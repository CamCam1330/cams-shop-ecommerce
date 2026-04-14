using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020670.BusinessLayers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SV22T1020670.Admin;
using SV22T1020670.Admin.AppCodes;
using SV22T1020670.Admin.Models;
using SV22T1020670.DomainModels.Models;
using SV22T1020670.DomainModels;
namespace SV22T1020670.Admin.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        public const int PAGESIZE = 10;
        private const string PRODUCT_SEARCH_CONDITION = "ProductSearchCondition";

        // ==================================================================
        // PHẦN 1: QUẢN LÝ THÔNG TIN CHUNG (PRODUCT)
        // ==================================================================

        public IActionResult Index()
        {
            var condition = ApplicationContext.GetSessionData<ProductSearchInput>(PRODUCT_SEARCH_CONDITION);
            if (condition == null)
            {
                condition = new ProductSearchInput()
                {
                    Page = 1,
                    PageSize = PAGESIZE,
                    SearchValue = "",
                    CategoryID = 0,
                    SupplierID = 0,
                    MinPrice = 0,
                    MaxPrice = 0
                };
            }
            return View(condition);
        }

        public async Task<IActionResult> Search(ProductSearchInput condition)
        {
            var data = await CommonDataService.ProductDB.ListAsync(
                condition.Page,
                condition.PageSize,
                condition.SearchValue ?? "",
                condition.CategoryID,
                condition.SupplierID,
                condition.MinPrice,
                condition.MaxPrice
            );

            var rowCount = await CommonDataService.ProductDB.CountAsync(
                condition.SearchValue ?? "",
                condition.CategoryID,
                condition.SupplierID,
                condition.MinPrice,
                condition.MaxPrice
            );

            var model = new PaginationSearchResult<Product>()
            {
                Page = condition.Page,
                PageSize = condition.PageSize,
                SearchValue = condition.SearchValue ?? string.Empty,
                RowCount = rowCount,
                Data = data.ToList()
            };

            ApplicationContext.SetSessionData(PRODUCT_SEARCH_CONDITION, condition);

            return PartialView("Search", model);
        }

        private async Task LoadDropdowns()
        {
            ViewBag.CategoryList = await CommonDataService.CategoryDB.ListAsync(1, 0, "");
            ViewBag.SupplierList = await CommonDataService.SupplierDB.ListAsync(1, 0, "");
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Title = "Bổ sung mặt hàng";
            await LoadDropdowns();
            var model = new Product()
            {
                ProductID = 0,
                Photo = "nophoto.png",
                IsSelling = true,
                Price = 0,
                SalePrice = 0 // Mặc định giá KM bằng 0
            };
            return View("Edit", model);
        }

        public async Task<IActionResult> Edit(int id = 0)
        {
            ViewBag.Title = "Cập nhật thông tin mặt hàng";
            var model = await CommonDataService.ProductDB.GetAsync(id);
            if (model == null)
                return RedirectToAction("Index");

            await LoadDropdowns();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveData(Product data, IFormFile? uploadPhoto)
        {
            try
            {
                ViewBag.Title = data.ProductID == 0 ? "Bổ sung mặt hàng" : "Cập nhật thông tin mặt hàng";

                // 1. Kiểm tra dữ liệu đầu vào (Validation)
                if (string.IsNullOrWhiteSpace(data.ProductName))
                    ModelState.AddModelError(nameof(data.ProductName), "Tên mặt hàng không được để trống");
                if (string.IsNullOrWhiteSpace(data.Unit))
                    ModelState.AddModelError(nameof(data.Unit), "Đơn vị tính không được để trống");
                if (data.CategoryID == 0)
                    ModelState.AddModelError(nameof(data.CategoryID), "Vui lòng chọn loại hàng");
                if (data.SupplierID == 0)
                    ModelState.AddModelError(nameof(data.SupplierID), "Vui lòng chọn nhà cung cấp");
                if (data.Price < 0)
                    ModelState.AddModelError(nameof(data.Price), "Giá hàng không được âm");
                if (data.SalePrice < 0)
                    ModelState.AddModelError(nameof(data.SalePrice), "Giá khuyến mãi không được âm");
                if (data.SalePrice > data.Price)
                    ModelState.AddModelError(nameof(data.SalePrice), "Giá khuyến mãi không được lớn hơn giá gốc");

                if (!ModelState.IsValid)
                {
                    await LoadDropdowns();
                    return View("Edit", data);
                }

                // 2. Xử lý upload ảnh (Theo phong cách EmployeeController + Fix lỗi Null)
                if (uploadPhoto != null)
                {
                    // Tên file: tick_tenfile.jpg
                    string fileName = $"{DateTime.Now.Ticks}_{uploadPhoto.FileName}";

                    // Đường dẫn lưu: wwwroot/images/products
                    string folder = Path.Combine(ApplicationContext.WWWRootPath, @"images\products");

                    // Tạo thư mục nếu chưa có (Fix warning CS8604)
                    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                    // Đường dẫn file đầy đủ
                    string filePath = Path.Combine(folder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await uploadPhoto.CopyToAsync(stream);
                    }

                    // Cập nhật tên ảnh vào model
                    data.Photo = fileName;
                }

                // 3. Gọi hàm xử lý Business
                if (data.ProductID == 0)
                    await CommonDataService.ProductDB.AddAsync(data);
                else
                    await CommonDataService.ProductDB.UpdateAsync(data);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Error", ex.Message);
                await LoadDropdowns();
                return View("Edit", data);
            }
        }

        public async Task<IActionResult> Delete(int id = 0)
        {
            if (Request.Method == "POST")
            {
                bool isInUse = await CommonDataService.ProductDB.InUsedAsync(id);
                if (isInUse)
                {
                    TempData["ErrorMessage"] = "Hàng hóa này đang được sử dụng trong đơn hàng, không thể xóa!";
                    return RedirectToAction("Index");
                }

                await CommonDataService.ProductDB.DeleteAsync(id);
                return RedirectToAction("Index");
            }

            var model = await CommonDataService.ProductDB.GetAsync(id);
            if (model == null) return RedirectToAction("Index");

            bool productInUse = await CommonDataService.ProductDB.InUsedAsync(id);
            if (productInUse)
            {
                ViewBag.ErrorMessage = "Hàng hóa này đang được sử dụng trong đơn hàng, không thể xóa!";
            }

            return View(model);
        }

        // ==================================================================
        // PHẦN 2 & 3: PHOTO & ATTRIBUTE (Giữ nguyên không thay đổi)
        // ==================================================================

        // ... (Giữ nguyên phần code Photo và Attribute bên dưới của bạn) ...
        // Bạn copy lại phần dưới của file cũ vào đây nhé vì phần đó không cần sửa gì cả.

        public async Task<IActionResult> Photo(int id = 0, string method = "", int photoId = 0)
        {
            // (Copy y nguyên code cũ của bạn vào đây)
            // ...
            switch (method)
            {
                case "add":
                    ViewBag.Title = "Bổ sung ảnh cho mặt hàng";
                    var model = new ProductPhoto()
                    {
                        PhotoID = 0,
                        ProductID = id,
                        Photo = "nophoto.png",
                        IsHidden = false,
                        DisplayOrder = 1
                    };
                    return View(model);

                case "edit":
                    ViewBag.Title = "Thay đổi ảnh của mặt hàng";
                    var photo = await CommonDataService.ProductDB.GetPhotoAsync(photoId);
                    if (photo == null) return RedirectToAction("Edit", new { id = id });
                    return View(photo);

                case "delete":
                    await CommonDataService.ProductDB.DeletePhotoAsync(photoId);
                    return RedirectToAction("Edit", new { id = id });

                default:
                    return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SavePhoto(ProductPhoto data, IFormFile? uploadPhoto)
        {
            // (Copy y nguyên code cũ của bạn vào đây)
            try
            {
                if (data.DisplayOrder <= 0)
                    ModelState.AddModelError(nameof(data.DisplayOrder), "Thứ tự hiển thị phải lớn hơn 0");

                if (!ModelState.IsValid)
                {
                    ViewBag.Title = data.PhotoID == 0 ? "Bổ sung ảnh" : "Thay đổi ảnh";
                    return View("Photo", data);
                }

                if (uploadPhoto != null)
                {
                    string fileName = $"{DateTime.Now.Ticks}_{uploadPhoto.FileName}";
                    string filePath = Path.Combine(ApplicationContext.WWWRootPath, @"images\products", fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await uploadPhoto.CopyToAsync(stream);
                    }
                    data.Photo = fileName;
                }
                else if (data.PhotoID == 0)
                {
                    ModelState.AddModelError(nameof(data.Photo), "Vui lòng chọn hình ảnh");
                    return View("Photo", data);
                }

                if (data.PhotoID == 0)
                    await CommonDataService.ProductDB.AddPhotoAsync(data);
                else
                    await CommonDataService.ProductDB.UpdatePhotoAsync(data);

                return RedirectToAction("Edit", new { id = data.ProductID });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Photo", data);
            }
        }

        public async Task<IActionResult> Attribute(int id = 0, string method = "", int attributeId = 0)
        {
            // (Copy y nguyên code cũ của bạn vào đây)
            switch (method)
            {
                case "add":
                    ViewBag.Title = "Bổ sung thuộc tính cho mặt hàng";
                    var model = new ProductAttribute()
                    {
                        AttributeID = 0,
                        ProductID = id,
                        DisplayOrder = 1
                    };
                    return View(model);

                case "edit":
                    ViewBag.Title = "Thay đổi thuộc tính của mặt hàng";
                    var attr = await CommonDataService.ProductDB.GetAttributeAsync(attributeId);
                    if (attr == null) return RedirectToAction("Edit", new { id = id });
                    return View(attr);

                case "delete":
                    await CommonDataService.ProductDB.DeleteAttributeAsync(attributeId);
                    return RedirectToAction("Edit", new { id = id });

                default:
                    return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveAttribute(ProductAttribute data)
        {
            // (Copy y nguyên code cũ của bạn vào đây)
            try
            {
                if (string.IsNullOrWhiteSpace(data.AttributeName))
                    ModelState.AddModelError(nameof(data.AttributeName), "Tên thuộc tính không được để trống");
                if (string.IsNullOrWhiteSpace(data.AttributeValue))
                    ModelState.AddModelError(nameof(data.AttributeValue), "Giá trị thuộc tính không được để trống");
                if (data.DisplayOrder <= 0)
                    ModelState.AddModelError(nameof(data.DisplayOrder), "Thứ tự hiển thị phải lớn hơn 0");

                if (!ModelState.IsValid)
                {
                    ViewBag.Title = data.AttributeID == 0 ? "Bổ sung thuộc tính" : "Thay đổi thuộc tính";
                    return View("Attribute", data);
                }

                if (data.AttributeID == 0)
                    await CommonDataService.ProductDB.AddAttributeAsync(data);
                else
                    await CommonDataService.ProductDB.UpdateAttributeAsync(data);

                return RedirectToAction("Edit", new { id = data.ProductID });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Attribute", data);
            }
        }
    }
}