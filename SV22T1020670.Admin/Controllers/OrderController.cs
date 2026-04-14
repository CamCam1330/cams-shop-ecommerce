using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using SV22T1020670.DomainModels;
using SV22T1020670.BusinessLayers;
using SV22T1020670.Admin.AppCodes;
using SV22T1020670.DomainModels.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Constants = SV22T1020670.DomainModels.Constants;
using SV22T1020670.Admin.Models;

namespace SV22T1020670.Admin.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private const string DATE_FORMAT = "dd/MM/yyyy";

        // Hằng số cho tìm kiếm mặt hàng
        private const string PRODUCT_SEARCH_FOR_SALE = "ProductSearchForSale";
        public const int PRODUCT_PAGE_SIZE = 5;
        private const string CART = "CART";

        // ==================================================================
        // PHẦN 1: QUẢN LÝ DANH SÁCH ĐƠN HÀNG (INDEX) - CODE MỚI THÊM VÀO
        // ==================================================================
        public async Task<IActionResult> Index(OrderSearchInput condition)
        {
            if (condition.PageSize <= 0) condition.PageSize = 20;

            // Gọi DAL trực tiếp với Property thông minh của Model
            // Không cần xử lý Split, Parse, TryCatch ở đây nữa!
            int rowCount = await OrderDataService.OrderDB.CountAsync(condition.Status, condition.FromDate, condition.ToDate, condition.SearchValue);
            var data = await OrderDataService.OrderDB.ListAsync(condition.Page, condition.PageSize, condition.Status, condition.FromDate, condition.ToDate, condition.SearchValue);

            var model = new PaginationSearchResult<Order>()
            {
                Page = condition.Page,
                PageSize = condition.PageSize,
                SearchValue = condition.SearchValue,
                RowCount = rowCount,
                Data = data
            };

            ViewBag.Condition = condition;
            return View(model);
        }

        // ==================================================================
        // PHẦN 2: LẬP ĐƠN HÀNG (CREATE & CART)
        // ==================================================================

        // Tạo mới đơn hàng
        public IActionResult Create()
        {
            // Khởi tạo giỏ hàng rỗng nếu chưa có
            var cart = GetSessionCart();
            if (cart == null || cart.Count == 0)
            {
                ApplicationContext.SetSessionData(CART, new List<OrderDetail>());
            }

            var condition = ApplicationContext.GetSessionData<ProductSearchInput>(PRODUCT_SEARCH_FOR_SALE);
            if (condition == null)
            {
                condition = new ProductSearchInput()
                {
                    Page = 1,
                    PageSize = PRODUCT_PAGE_SIZE,
                    SearchValue = "",
                    CategoryID = 0,
                    SupplierID = 0,
                    MaxPrice = 0,
                    MinPrice = 0
                };
            }
            return View(condition);
        }


        /// <summary>
        /// Tìm kiếm mặt hàng để đưa vào giỏ
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> SearchProducts(ProductSearchInput condition)
        {
            if (condition == null)
                return Content("Yêu cầu không hợp lệ");

            var data = await ProductDataService.ProductDB.ListAsync(
                                        condition.Page,
                                        condition.PageSize,
                                        condition.SearchValue ?? "",
                                        condition.CategoryID,
                                        condition.SupplierID,
                                        condition.MinPrice,
                                        condition.MaxPrice);

            var rowCount = await ProductDataService.ProductDB.CountAsync(
                                        condition.SearchValue ?? "",
                                        condition.CategoryID,
                                        condition.SupplierID,
                                        condition.MinPrice,
                                        condition.MaxPrice);

            var model = new ProductSearchResult()
            {
                Page = condition.Page,
                PageSize = condition.PageSize,
                SearchValues = condition.SearchValue ?? "",
                CategoryId = condition.CategoryID,
                SupplierId = condition.SupplierID,
                MaxPrice = condition.MaxPrice,
                MinPrice = condition.MinPrice,
                Data = data,
                RowCount = rowCount
            };

            // Lưu điều kiện tìm kiếm vào Session để dùng lại nếu cần
            ApplicationContext.SetSessionData(PRODUCT_SEARCH_FOR_SALE, condition);

            return PartialView("SearchProducts", model);
        }

        public async Task<IActionResult> Init(string customerID, string deliveryProvince, string deliveryAddress)
        {
            try 
            {
                int orderID = 0;

                var cart = GetSessionCart();
                if (cart.Count ==0)
                    return Json(new ApiResult { Code = 0, Message = "Không thể lập đơn hàng vì giỏ hàng đang trống" });
                if (string.IsNullOrWhiteSpace(customerID))
                    return Json(new ApiResult { Code = 0, Message = "Vui lòng chọn khách hàng" });
                if (string.IsNullOrWhiteSpace(deliveryProvince))
                    return Json(new ApiResult { Code = 0, Message = "Vui lòng chọn tỉnh thành giao hàng" });
                if (string.IsNullOrWhiteSpace(deliveryAddress))
                    return Json(new ApiResult { Code = 0, Message = "Vui lòng nhập địa chỉ giao hàng" });

                Order data = new Order()
                {
                    CustomerID = Convert.ToInt32(customerID),
                    DeliveryProvince = deliveryProvince,
                    DeliveryAddress = deliveryAddress,
                    EmployeeID = Convert.ToInt32(User.GetUserData().UserId),
                    Status = SV22T1020670.DomainModels.Constants.ORDER_INIT
                };
                orderID = await OrderDataService.OrderDB.AddAsync(data);
                foreach (var item in cart)
                {
                    await OrderDataService.OrderDB.SaveDetailAsync(orderID, item.ProductID, item.Quantity, item.SalePrice);
                }

                return Json(new ApiResult { Code = 1, Message = "", Data = orderID });
            }
            catch (Exception ex) {
                return Json(new ApiResult { Code = 0, Message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy giỏ hàng đang có trong session
        /// </summary>
        /// <returns></returns>
        private List<OrderDetail> GetSessionCart()
        {
            var cart = ApplicationContext.GetSessionData<List<OrderDetail>>(CART);
            if (cart == null)
            {
                cart = new List<OrderDetail>();
            }
            return cart;
        }

        /// <summary>
        /// Lấy giỏ hàng hiện có
        /// </summary>
        /// <returns></returns>
        public IActionResult GetCart()
        {
            return PartialView("GetCart", GetSessionCart());
        }

        /// <summary>
        /// Thêm sản phẩm vào giỏ hàng
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult AddToCart(OrderDetail data)
        {
            if (data.Quantity < 1)
                return Json(new ApiResult() { Code = 0, Message = "Số lượng không hợp lệ" });
            if (data.SalePrice < 0)
                return Json(new ApiResult() { Code = 0, Message = "Giá bán không hợp lệ" });

            AddSessionCart(data);
            return Json(new ApiResult() { Code = 1 });
        }
        
        private void AddSessionCart(OrderDetail data)
        {
            var cart = GetSessionCart();
            var existOrderDetails = cart.Find(m => m.ProductID == data.ProductID);
            if (existOrderDetails == null)
            {
                cart.Add(data);
            }
            else
            {
                existOrderDetails.Quantity += data.Quantity;
                existOrderDetails.SalePrice = data.SalePrice;
            }
            ApplicationContext.SetSessionData(CART, cart);
        }

        /// <summary>
        /// Xóa mặt hàng có mã id ra khỏi giỏ hàng
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult RemoveFromCart(int id)
        {
            try
            {
                var cart = GetSessionCart();
                int index = cart.FindIndex(m => m.ProductID == id);
                if (index >= 0)
                {
                    cart.RemoveAt(index);
                    ApplicationContext.SetSessionData(CART, cart);
                    return Json(new ApiResult() { Code = 1 });
                }
                return Json(new ApiResult() { Code = 0, Message = "Mặt hàng không tồn tại" });
            }
            catch (Exception ex)
            {
                return Json(new ApiResult() { Code = 0, Message = ex.Message });
            }
        }

        /// <summary>
        /// Xóa toàn bộ giỏ hàng
        /// </summary>
        /// <returns></returns>
        public IActionResult ClearCart()
        {
            try 
            {
                var cart = GetSessionCart();
                cart.Clear();
                ApplicationContext.SetSessionData(CART, cart);
                return Json(new ApiResult() { Code = 1 });

            }
            catch (Exception ex)
            {
                return Json(new ApiResult() { Code = 0, Message = ex.Message });
            }
        }

        /// <summary>
        /// Giảm số lượng hàng trong giỏ
        /// </summary>
        /// <param name="id"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult UpdateCartQuantity(int id, int quantity)
        {
            try 
            {
                var cart = GetSessionCart();
                var existsProduct = cart.FindLast(m => m.ProductID == id);
                if (existsProduct != null)
                {
                    int remainQuantity = existsProduct.Quantity + quantity;
                    if (remainQuantity <= 0)
                    {
                        return Json(new ApiResult() { Code = 0, Message = "Số lượng còn lại không hợp lệ"});
                    }

                    existsProduct.Quantity = existsProduct.Quantity + quantity;
                    ApplicationContext.SetSessionData(CART, cart);
                    return Json(new ApiResult() { Code = 1});
                }
                return Json(new ApiResult() { Code = 0, Message = "Mặt hàng không tồn tại" });
            }
            catch (Exception ex) 
            { 
                return Json(new ApiResult() {Code = 0, Message = ex.Message});
            }
        }


        /// <summary>
        /// Lưu đơn hàng mới (Order và OrderDetails) trong một transaction
        /// </summary>
        /// <param name="customerID"></param>
        /// <param name="deliveryProvince"></param>
        /// <param name="deliveryAddress"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveOrder(int customerID, string deliveryProvince, string deliveryAddress)
        {
            try
            {
                // Kiểm tra dữ liệu đầu vào
                if (customerID == 0)
                {
                    ModelState.AddModelError("CustomerID", "Chọn khách hàng");
                }
                if (string.IsNullOrWhiteSpace(deliveryProvince))
                {
                    ModelState.AddModelError("DeliveryProvince", "Chọn tỉnh/thành");
                }
                if (string.IsNullOrWhiteSpace(deliveryAddress))
                {
                    ModelState.AddModelError("DeliveryAddress", "Nhập địa chỉ nhận hàng");
                }

                var cart = GetSessionCart();
                if (cart == null || cart.Count == 0)
                {
                    ModelState.AddModelError("Cart", "Giỏ hàng trống. Vui lòng thêm sản phẩm vào giỏ hàng");
                }

                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "Vui lòng kiểm tra lại thông tin đơn hàng";
                    return RedirectToAction("Create");
                }

                // Lấy thông tin nhân viên từ session
                var userData = User.GetUserData();
                int? employeeID = null;
                if (userData != null && !string.IsNullOrEmpty(userData.UserId))
                {
                    if (int.TryParse(userData.UserId, out int empId))
                    {
                        employeeID = empId;
                    }
                }

                // Tạo đơn hàng mới
                var order = new Order
                {
                    CustomerID = customerID,
                    DeliveryProvince = deliveryProvince,
                    DeliveryAddress = deliveryAddress,
                    EmployeeID = employeeID,
                    Status = SV22T1020670.DomainModels.Constants.ORDER_INIT,
                    OrderTime = DateTime.Now
                };

                // Lưu đơn hàng và chi tiết trong một transaction
                int orderID = await OrderDataService.OrderDB.SaveOrderWithDetailsAsync(order, cart ?? new List<OrderDetail>());

                // Xóa giỏ hàng sau khi lưu thành công
                ClearCart();

                TempData["Message"] = "Lập đơn hàng thành công!";
                return RedirectToAction("Details", new { id = orderID });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi lưu đơn hàng: " + ex.Message;
                return RedirectToAction("Create");
            }
        }


        // ==================================================================
        // PHẦN 3: CHI TIẾT VÀ XỬ LÝ ĐƠN HÀNG (DETAILS & ACTIONS)
        // ==================================================================



        // Chi tiết đơn hàng
        public async Task<IActionResult> Details(int id = 0)
        {
            // 1. Lấy thông tin đơn hàng (Sd hàm GetAsync từ DAL)
            var order = await OrderDataService.OrderDB.GetAsync(id);

            if (order == null)
            {
                return RedirectToAction("Index");
            }
            // 2. Lấy chi tiết đơn hàng (hàm ListDetailsAsync)
            var details = await OrderDataService.OrderDB.ListDetailsAsync(id);

            // 3. Đưa dữ liệu vào Model
            var model = new SV22T1020670.Admin.Models.OrderDetailModel()
            {
                Order = order,
                Details = details.ToList() // Chuyển IEnumerable sang List
            };

            return View(model);
        }

        // Modal: chỉnh sửa chi tiết đơn hàng
        public async Task<IActionResult> EditDetail(int id, int productId)
        {
            var order = await OrderDataService.OrderDB.GetAsync(id);
            // Chỉ cho sửa khi đơn hàng Mới (1) hoặc Đã duyệt (2)
            if (order == null || order.Status == Constants.ORDER_SHIPPING || order.Status == Constants.ORDER_FINISHED)
            {
                return RedirectToAction("Details", new { id = id });
            }

            var detail = await OrderDataService.OrderDB.GetDetailAsync(id, productId);
            if (detail == null) return RedirectToAction("Details", new { id = id });
            return View(detail);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateDetail(int id, int productId, int quantity, decimal salePrice)
        {
            var order = await OrderDataService.OrderDB.GetAsync(id);
            // Chặn sửa đổi nếu đơn hàng đã đi giao
            if (order == null || order.Status == Constants.ORDER_SHIPPING || order.Status == Constants.ORDER_FINISHED)
            {
                return RedirectToAction("Details", new { id = id });
            }

            if (quantity <= 0 || salePrice < 0) return RedirectToAction("Details", new { id = id });

            await OrderDataService.OrderDB.SaveDetailAsync(id, productId, quantity, salePrice);
            return RedirectToAction("Details", new { id = id });
        }


        /// <summary>
        /// Chuyển giao hàng: Chỉ thực hiện khi Đã duyệt (2) hoặc đang Vận chuyển (3) (để cập nhật lại shipper)
        /// </summary>
        public IActionResult Shipping(int id)
        {
            ViewBag.OrderID = id;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Shipping(int id, int shipperID)
        {
            var order = await OrderDataService.OrderDB.GetAsync(id);
            // Cho phép cập nhật shipper ngay cả khi đang giao hàng
            if (order != null && (order.Status == Constants.ORDER_ACCEPTED || order.Status == Constants.ORDER_SHIPPING))
            {
                order.Status = Constants.ORDER_SHIPPING;
                order.ShipperID = shipperID;
                order.ShippedTime = DateTime.Now;
                await OrderDataService.OrderDB.UpdateAsync(order);
            }
            return RedirectToAction("Details", new { id = id });
        }

        /// <summary>
        /// Duyệt đơn hàng: Chỉ thực hiện khi đơn hàng đang ở trạng thái Init (1)
        /// </summary>
        /// <summary>
        /// Duyệt chấp nhận đơn hàng
        /// </summary>
        public async Task<IActionResult> Accept(int id)
        {
            // 1. Kiểm tra đơn hàng tồn tại không
            var order = await OrderDataService.OrderDB.GetAsync(id);
            if (order == null)
                return RedirectToAction("Index");

            // 2. Lấy chi tiết các mặt hàng trong đơn
            var details = await OrderDataService.OrderDB.ListDetailsAsync(id);

            // 3. [QUAN TRỌNG] Lặp qua từng món để trừ tồn kho
            foreach (var item in details)
            {
                // Gọi hàm DecreaseStockAsync mà chúng ta vừa viết ở ProductDAL
                // Hàm này sẽ trả về False nếu số lượng tồn < số lượng mua
                bool isStockDeducted = await ProductDataService.ProductDB.DecreaseStockAsync(item.ProductID, item.Quantity);

                if (!isStockDeducted)
                {
                    // TÌNH HUỐNG HẾT HÀNG:
                    // Nếu không trừ được (do kho thiếu hàng), ta dừng ngay lập tức.
                    // Không duyệt đơn nữa và báo lỗi cho Admin biết.
                    TempData["Message"] = $"Duyệt thất bại! Sản phẩm '{item.ProductName}' không đủ số lượng tồn kho.";
                    return RedirectToAction("Details", new { id = id });
                }
            }

            // 4. Nếu tất cả sản phẩm đều trừ kho thành công -> Cập nhật trạng thái đơn hàng
            bool result = await OrderDataService.OrderDB.AcceptOrderAsync(id);

            if (result)
                TempData["Message"] = "Đã duyệt đơn hàng và trừ kho thành công!";
            else
                TempData["Message"] = "Duyệt đơn hàng thất bại (Lỗi hệ thống).";

            return RedirectToAction("Details", new { id = id });
        }

        /// <summary>
        /// Hoàn tất đơn hàng: Chỉ thực hiện khi đang Vận chuyển (3)
        /// </summary>
        public async Task<IActionResult> Finish(int id)
        {
            var order = await OrderDataService.OrderDB.GetAsync(id);
            if (order != null && order.Status == Constants.ORDER_SHIPPING)
            {
                order.Status = Constants.ORDER_FINISHED;
                order.FinishedTime = DateTime.Now;
                await OrderDataService.OrderDB.UpdateAsync(order);
            }
            return RedirectToAction("Details", new { id = id });
        }

        /// <summary>
        /// Hủy đơn hàng: Được phép hủy khi Mới (1), Đã duyệt (2), hoặc Giao thất bại (3)
        /// Không được hủy khi đã Hoàn tất (4)
        /// </summary>
        public async Task<IActionResult> Cancel(int id)
        {
            var order = await OrderDataService.OrderDB.GetAsync(id);
            if (order != null && order.Status != Constants.ORDER_FINISHED)
            {
                order.Status = Constants.ORDER_CANCEL;
                order.FinishedTime = DateTime.Now;
                await OrderDataService.OrderDB.UpdateAsync(order);
            }
            return RedirectToAction("Details", new { id = id });
        }

        /// <summary>
        /// Từ chối đơn hàng: Chỉ thực hiện khi đơn Mới (1)
        /// </summary>
        public async Task<IActionResult> Reject(int id)
        {
            var order = await OrderDataService.OrderDB.GetAsync(id);
            if (order != null && order.Status == Constants.ORDER_INIT)
            {
                order.Status = Constants.ORDER_REJECTED;
                order.FinishedTime = DateTime.Now;
                await OrderDataService.OrderDB.UpdateAsync(order);
            }
            return RedirectToAction("Details", new { id = id });
        }

        /// <summary>
        /// Xóa đơn hàng: 
        /// - Chỉ xóa được khi đơn hàng ở trạng thái: Mới (1), Hủy (-1), Từ chối (-2)
        /// - CẤM xóa khi: Đã duyệt (2), Đang giao (3), Hoàn tất (4)
        /// </summary>
        public async Task<IActionResult> Delete(int id)
        {
            var order = await OrderDataService.OrderDB.GetAsync(id);
            if (order != null)
            {
                // Kiểm tra điều kiện xóa
                if (order.Status == Constants.ORDER_INIT
                    || order.Status == Constants.ORDER_CANCEL
                    || order.Status == Constants.ORDER_REJECTED)
                {
                    await OrderDataService.OrderDB.DeleteAsync(id);
                    return RedirectToAction("Index");
                }
            }
            // Nếu không xóa được thì quay lại trang chi tiết (hoặc thông báo lỗi)
            return RedirectToAction("Details", new { id = id });
        }

        //Xóa chi tiết đơn hàng
        public async Task<IActionResult> DeleteDetail(int id, int productId)
        {
            var order = await OrderDataService.OrderDB.GetAsync(id);
            // Chặn xóa chi tiết nếu đơn hàng đã đi giao
            if (order == null || order.Status == Constants.ORDER_SHIPPING || order.Status == Constants.ORDER_FINISHED)
            {
                return RedirectToAction("Details", new { id = id });
            }

            await OrderDataService.OrderDB.DeleteDetailAsync(id, productId);
            return RedirectToAction("Details", new { id = id });
        }
    }
}
