using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020670.BusinessLayers;
using SV22T1020670.Shop.AppCodes;
using SV22T1020670.Shop.Models;
using SV22T1020670.DomainModels; // Needed for Constants
using System; // Needed for DateTime
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SV22T1020670.Shop.Controllers
{
    public class OrderController : Controller
    {
        public const string SHOP_CART = "SHOP_CART";

        // --- PRIVATE HELPERS ---

        private List<CartItem> GetShoppingCart()
        {
            var cart = ApplicationContext.GetSessionData<List<CartItem>>(SHOP_CART);
            if (cart == null)
            {
                cart = new List<CartItem>();
                ApplicationContext.SetSessionData(SHOP_CART, cart);
            }
            return cart;
        }

        private void SaveShoppingCart(List<CartItem> shoppingCart)
        {
            ApplicationContext.SetSessionData(SHOP_CART, shoppingCart);
        }

        // --- CART ACTIONS ---

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productID, int quantity = 1)
        {
            // 1. Validate input
            if (productID <= 0 || quantity <= 0)
            {
                return Json(new { success = false, code = 0, message = "Thông tin không hợp lệ!" });
            }

            // 2. Lấy thông tin sản phẩm
            var product = await ProductDataService.ProductDB.GetAsync(productID);
            if (product == null)
            {
                return Json(new { success = false, code = 0, message = "Sản phẩm không tồn tại!" });
            }

            // 3. Kiểm tra tồn kho (CRITICAL: E-commerce business logic)
            if (product.Quantity <= 0)
            {
                return Json(new { success = false, code = 0, message = "Sản phẩm này hiện đang tạm hết hàng!" });
            }

            // 4. Kiểm tra số lượng yêu cầu không vượt quá tồn kho
            var shoppingCart = GetShoppingCart();
            var existsItem = shoppingCart.FirstOrDefault(m => m.ProductID == productID);
            int currentCartQuantity = existsItem?.Quantity ?? 0;
            int requestedTotal = currentCartQuantity + quantity;

            if (requestedTotal > product.Quantity)
            {
                int available = product.Quantity - currentCartQuantity;
                if (available <= 0)
                {
                    return Json(new { success = false, code = 0, message = $"Bạn đã có {currentCartQuantity} sản phẩm trong giỏ. Trong kho chỉ còn {product.Quantity} sản phẩm!" });
                }
                return Json(new { success = false, code = 0, message = $"Trong kho chỉ còn {product.Quantity} sản phẩm. Bạn có thể thêm tối đa {available} sản phẩm nữa!" });
            }

            // 5. Tính giá thực tế (SalePrice logic)
            decimal realPrice = product.Price;
            if (product.SalePrice > 0 && product.SalePrice < product.Price)
            {
                realPrice = product.SalePrice;
            }

            // 6. Cập nhật hoặc thêm mới vào giỏ hàng
            if (existsItem != null)
            {
                existsItem.Quantity += quantity;
            }
            else
            {
                var newItem = new CartItem()
                {
                    ProductID = product.ProductID,
                    ProductName = product.ProductName,
                    Photo = product.Photo,
                    Unit = product.Unit,
                    Price = realPrice,
                    Quantity = quantity
                };
                shoppingCart.Add(newItem);
            }

            SaveShoppingCart(shoppingCart);
            int totalQty = shoppingCart.Sum(m => m.Quantity);
            
            return Json(new { success = true, code = 1, message = "Đã thêm sản phẩm vào giỏ hàng!", cartItemCount = totalQty });
        }

        public IActionResult ShoppingCart()
        {
            return View(GetShoppingCart());
        }

        public IActionResult RemoveFromCart(int id)
        {
            var cart = GetShoppingCart();
            var index = cart.FindIndex(m => m.ProductID == id);
            if (index >= 0)
            {
                cart.RemoveAt(index);
                SaveShoppingCart(cart);
            }
            return RedirectToAction("ShoppingCart");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int id, int quantity)
        {
            try
            {
                // 1. Validate input
                if (id <= 0)
                {
                    return Json(new { success = false, Code = 0, Message = "Mặt hàng không tồn tại" });
                }

                var cart = GetShoppingCart();
                var item = cart.FirstOrDefault(m => m.ProductID == id);

                if (item == null)
                {
                    return Json(new { success = false, Code = 0, Message = "Mặt hàng không tồn tại trong giỏ hàng" });
                }

                int newQuantity = item.Quantity + quantity;

                // 2. Nếu số lượng <= 0 thì xóa khỏi giỏ
                if (newQuantity <= 0)
                {
                    cart.Remove(item);
                    SaveShoppingCart(cart);
                    decimal removedCartTotal = cart.Sum(x => x.TotalPrice);
                    int removedCartQty = cart.Sum(x => x.Quantity);
                    return Json(new 
                    { 
                        success = true, 
                        Code = 2, 
                        Message = "Đã xóa sản phẩm",
                        CartTotal = removedCartTotal.ToString("N0") + "đ",
                        CartQty = removedCartQty
                    });
                }

                // 3. CRITICAL: Kiểm tra tồn kho trước khi cập nhật
                var product = await ProductDataService.ProductDB.GetAsync(id);
                if (product == null)
                {
                    return Json(new { success = false, Code = 0, Message = "Sản phẩm không tồn tại trong hệ thống" });
                }

                if (product.Quantity <= 0)
                {
                    return Json(new { success = false, Code = 0, Message = "Sản phẩm này hiện đang tạm hết hàng!" });
                }

                if (newQuantity > product.Quantity)
                {
                    return Json(new { success = false, Code = 0, Message = $"Trong kho chỉ còn {product.Quantity} sản phẩm!" });
                }

                // 4. Cập nhật số lượng
                item.Quantity = newQuantity;
                SaveShoppingCart(cart);

                decimal itemTotalPrice = item.TotalPrice;
                decimal cartTotalPrice = cart.Sum(x => x.TotalPrice);
                int cartTotalQty = cart.Sum(x => x.Quantity);

                return Json(new
                {
                    success = true,
                    Code = 1,
                    Message = "Cập nhật thành công",
                    ItemID = id,
                    ItemTotal = itemTotalPrice.ToString("N0") + "đ",
                    CartTotal = cartTotalPrice.ToString("N0") + "đ",
                    CartQty = cartTotalQty
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, Code = 0, Message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        public IActionResult ClearCart()
        {
            var cart = GetShoppingCart();
            cart.Clear();
            SaveShoppingCart(cart);
            return RedirectToAction("ShoppingCart");
        }

        // --- CHECKOUT & ORDER ---

        public IActionResult Checkout()
        {
            var cart = GetShoppingCart();
            if (cart.Count == 0)
                return RedirectToAction("ShoppingCart");

            return View(cart);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> PlaceOrder(string deliveryName, string deliveryPhone, string deliveryEmail,
                                                    string deliveryProvince, string deliveryDistrict, string deliveryAddress,
                                                    string paymentMethod)
        {
            // 1. Kiểm tra giỏ hàng
            var cart = GetShoppingCart();
            if (cart.Count == 0)
            {
                TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống!";
                return RedirectToAction("ShoppingCart");
            }

            // 2. Validate thông tin giao hàng
            if (string.IsNullOrWhiteSpace(deliveryName) || string.IsNullOrWhiteSpace(deliveryPhone) || 
                string.IsNullOrWhiteSpace(deliveryAddress) || string.IsNullOrWhiteSpace(deliveryProvince))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập đầy đủ thông tin giao hàng!";
                return RedirectToAction("Checkout");
            }

            // 3. Lấy thông tin khách hàng
            var userData = User.GetUserData();
            if (userData == null || !int.TryParse(userData.UserId, out int customerID) || customerID <= 0)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để đặt hàng!";
                return RedirectToAction("Login", "Account");
            }

            // 4. CRITICAL: Kiểm tra tồn kho cho từng sản phẩm trong giỏ hàng
            var invalidItems = new List<string>();
            foreach (var item in cart)
            {
                var product = await ProductDataService.ProductDB.GetAsync(item.ProductID);
                if (product == null)
                {
                    invalidItems.Add($"{item.ProductName} - Sản phẩm không tồn tại");
                    continue;
                }

                if (product.Quantity <= 0)
                {
                    invalidItems.Add($"{item.ProductName} - Đã hết hàng");
                    continue;
                }

                if (item.Quantity > product.Quantity)
                {
                    invalidItems.Add($"{item.ProductName} - Chỉ còn {product.Quantity} sản phẩm trong kho (bạn đã chọn {item.Quantity})");
                }
            }

            if (invalidItems.Any())
            {
                TempData["ErrorMessage"] = "Một số sản phẩm trong giỏ hàng không còn đủ số lượng:\n" + string.Join("\n", invalidItems);
                return RedirectToAction("ShoppingCart");
            }

            // 5. Tạo đơn hàng
            var order = new SV22T1020670.DomainModels.Order()
            {
                CustomerID = customerID,
                OrderTime = DateTime.Now,
                DeliveryAddress = $"{deliveryName} ({deliveryPhone}) - {deliveryAddress}, {deliveryDistrict}, {deliveryProvince}",
                DeliveryProvince = deliveryProvince,
                EmployeeID = null,
                Status = Constants.ORDER_INIT
            };

            var details = new List<SV22T1020670.DomainModels.OrderDetail>();
            foreach (var item in cart)
            {
                details.Add(new SV22T1020670.DomainModels.OrderDetail()
                {
                    ProductID = item.ProductID,
                    Quantity = item.Quantity,
                    SalePrice = item.Price
                });
            }

            // 6. Lưu đơn hàng
            int orderID = await OrderDataService.OrderDB.SaveOrderWithDetailsAsync(order, details);

            if (orderID > 0)
            {
                ClearCart();
                return RedirectToAction("OrderSuccess", new { id = orderID });
            }
            else
            {
                TempData["ErrorMessage"] = "Lỗi hệ thống, không thể lưu đơn hàng. Vui lòng thử lại!";
                return RedirectToAction("Checkout");
            }
        }

        public IActionResult OrderSuccess(int id)
        {
            return View(id);
        }

        // --- TRACKING & HISTORY ---

        // GET: Search Form
        [HttpGet]
        public IActionResult Search()
        {
            return View();
        }

        // POST: Secure Search
        [HttpPost]
        public async Task<IActionResult> Search(int id, string phone)
        {
            // 1. Lấy đơn hàng
            var order = await OrderDataService.OrderDB.GetAsync(id);

            bool isMatch = false;

            if (order != null)
            {
                // --- LOGIC CHECK BẢO MẬT MỚI (MỞ RỘNG) ---

                // Cách 1: Check xem SĐT có nằm trong Địa chỉ giao hàng không (cho khách lẻ)
                if (!string.IsNullOrEmpty(order.DeliveryAddress) && order.DeliveryAddress.Contains(phone))
                {
                    isMatch = true;
                }
                // Cách 2: Check xem SĐT có khớp với SĐT trong hồ sơ khách hàng không (cho khách Demo/Thành viên)
                else if (!string.IsNullOrEmpty(order.CustomerPhone) && order.CustomerPhone == phone)
                {
                    isMatch = true;
                }
            }

            if (!isMatch)
            {
                TempData["Message"] = "Không tìm thấy đơn hàng hoặc Số điện thoại không khớp.";
                return View();
            }

            // Nếu đúng -> Lấy chi tiết và hiển thị
            var details = await OrderDataService.OrderDB.ListDetailsAsync(id);
            ViewBag.OrderDetails = details;

            return View("Tracking", order);
        }

        // Secure Tracking for Logged-in Users
        [Authorize]
        public async Task<IActionResult> Tracking(int id)
        {
            var order = await OrderDataService.OrderDB.GetAsync(id);
            if (order == null) return RedirectToAction("Index", "Home");

            var userData = User.GetUserData();
            if (userData == null) return RedirectToAction("Login", "Account");

            int currentUserID = int.Parse(userData.UserId);

            if (order.CustomerID != currentUserID)
            {
                return Content("Bạn không có quyền xem đơn hàng này!");
            }

            var details = await OrderDataService.OrderDB.ListDetailsAsync(id);
            ViewBag.OrderDetails = details;

            return View(order);
        }

        // Order History for Logged-in Users
        [Authorize]
        public async Task<IActionResult> History()
        {
            var userData = User.GetUserData();
            if (userData == null) return RedirectToAction("Login", "Account");

            int customerID = int.Parse(userData.UserId);
            var orders = await OrderDataService.OrderDB.ListByCustomerAsync(customerID);

            return View(orders);
        }
    }
}