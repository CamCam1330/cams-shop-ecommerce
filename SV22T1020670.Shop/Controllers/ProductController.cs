using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020670.BusinessLayers;
using SV22T1020670.DomainModels;
using SV22T1020670.Shop.AppCodes;
using SV22T1020670.Shop.Models; 
using System.Threading.Tasks;

namespace SV22T1020670.Shop.Controllers
{
    public class ProductController : Controller
    {
        private const int PAGE_SIZE = 20;

        // ==================================================================
        // TRANG DANH SÁCH SẢN PHẨM (INDEX)
        // ==================================================================
        public async Task<IActionResult> Index(string searchValue = "", int categoryID = 0, decimal minPrice = 0, decimal maxPrice = 0, int page = 1, string sortBy = "")
        {
            // 1. 
            // Task lấy danh sách sản phẩm
            var dataTask = CommonDataService.ProductDB.ListAsync(
                page, PAGE_SIZE, searchValue ?? "", categoryID, 0, minPrice, maxPrice, sortBy
            );

            // Task đếm tổng số dòng
            var countTask = CommonDataService.ProductDB.CountAsync(
                searchValue ?? "", categoryID, 0, minPrice, maxPrice
            );

            // Task lấy danh sách Category
            var categoryTask = CommonDataService.CategoryDB.ListAsync();

            await Task.WhenAll(dataTask, countTask, categoryTask);

            // 2. Lấy kết quả từ Task
            var data = await dataTask;
            var rowCount = await countTask;
            var categories = await categoryTask;

            // 3. Đổ dữ liệu Category vào ViewBag
            ViewBag.Categories = categories.ToList();

            // 4. Đóng gói dữ liệu ra View
            var model = new SV22T1020670.Shop.Models.ProductSearchOutput
            {
                Page = page,
                PageSize = PAGE_SIZE,
                SearchValue = searchValue ?? "",
                CategoryID = categoryID,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                SortBy = sortBy,
                RowCount = rowCount,
                Data = data.ToList()
            };

            return View(model);
        }

        // ==================================================================
        // TRANG CHI TIẾT SẢN PHẨM (DETAIL)
        // ==================================================================
        public async Task<IActionResult> Details(int id = 0)
        {
            if (id <= 0)
                return RedirectToAction("Index");

            // 1. Lấy thông tin sản phẩm (Async)
            var product = await CommonDataService.ProductDB.GetAsync(id);

            if (product == null)
                return RedirectToAction("Index");

            // 2. Lấy danh sách ảnh phụ (Gallery)
            var photos = await CommonDataService.ProductDB.ListPhotosAsync(id);

            // 3. Lấy thuộc tính (Attributes)
            var attributes = await CommonDataService.ProductDB.ListAttributesAsync(id);

            // 4. Lấy danh sách đánh giá
            var review = await ProductDataService.ProductDB.ListReviewsAsync(id);

            // 5. Truyền dữ liệu bổ sung qua ViewBag 
            ViewBag.Photos = photos;
            ViewBag.Attributes = attributes;
            ViewBag.Reviews = review;

            return View(product);
        }

        // ==================================================================
        // REVIEW SẢN PHẨM
        // ==================================================================
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Review(int productID, int rating, string comment)
        {
            // 1. Kiểm tra đăng nhập
            var user = User.GetUserData();
            if (user == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập để đánh giá!", code = -1 });
            }

            int.TryParse(user.UserId, out int customerId);

            // 2. CHECK ĐÃ MUA HÀNG CHƯA
            bool hasPurchased = await OrderDataService.OrderDB.HasPurchasedProductAsync(customerId, productID);

            if (!hasPurchased)
            {
                return Json(new
                {
                    success = false,
                    message = "Bạn cần mua và nhận hàng thành công sản phẩm này mới được đánh giá."
                });
            }

            // 3. Validate dữ liệu
            if (rating < 1) rating = 5;
            if (string.IsNullOrWhiteSpace(comment)) comment = "Sản phẩm tốt!";

            // 4. Lưu đánh giá
            var review = new DomainModels.ProductReview()
            {
                ProductID = productID,
                CustomerID = customerId,
                Rating = rating,
                Comment = comment,
                IsHidden = false
            };

            try
            {
                await ProductDataService.ProductDB.AddReviewAsync(review);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }

            return Json(new { success = true, message = "Cảm ơn bạn đã đánh giá!" });
        }
    }
}