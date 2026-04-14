using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020670.BusinessLayers;
using SV22T1020670.DomainModels;
using SV22T1020670.Admin.AppCodes;
using SV22T1020670.DomainModels.Models;

namespace SV22T1020670.Admin.Controllers
{
    [Authorize]
    public class CategoryController : Controller
    {
        public const int PAGESIZE = 18;
        private const string CATEGORY_SEARCH_CONDITION = "CategorySearchCondition";

        public IActionResult Index()
        {
            var condition = ApplicationContext.GetSessionData<PaginationSearchInput>(CATEGORY_SEARCH_CONDITION);
            if (condition == null)
            {
                condition = new PaginationSearchInput()
                {
                    Page = 1,
                    PageSize = PAGESIZE,
                    SearchValue = ""
                };
            }
            return View(condition);
        }

        public async Task<IActionResult> Search(PaginationSearchInput condition)
        {
            ApplicationContext.SetSessionData(CATEGORY_SEARCH_CONDITION, condition);

            var data = await CommonDataService.CategoryDB.ListAsync(condition.Page, condition.PageSize, condition.SearchValue);
            var rowCount = await CommonDataService.CategoryDB.CountAsync(condition.SearchValue);

            var model = new PaginationSearchResult<Category>()
            {
                Page = condition.Page,
                PageSize = condition.PageSize,
                SearchValue = condition.SearchValue,
                RowCount = rowCount,
                Data = data
            };

            return PartialView(model);
        }

        public IActionResult Create()
        {
            ViewBag.Title = "Bổ sung loại hàng";
            var model = new Category()
            {
                CategoryID = 0,
                Photo = "nophoto.png" // Mặc định chưa có ảnh
            };
            return View("Edit", model);
        }

        public async Task<IActionResult> Edit(int id = 0)
        {
            ViewBag.Title = "Chỉnh sửa thông tin loại hàng";
            var model = await CommonDataService.CategoryDB.GetAsync(id);
            if (model == null)
                return RedirectToAction("Index");
            return View(model);
        }

        [HttpPost]
        // 👇 CẬP NHẬT: Thêm tham số uploadPhoto để nhận file từ form
        [HttpPost]
        public async Task<IActionResult> SaveData(Category data, IFormFile? uploadPhoto)
        {
            try
            {
                ViewBag.Title = data.CategoryID == 0 ? "Bổ sung loại hàng" : "Chỉnh sửa thông tin loại hàng";

                if (string.IsNullOrWhiteSpace(data.CategoryName))
                    ModelState.AddModelError(nameof(data.CategoryName), "Tên loại hàng không được để trống");

                // Xử lý upload ảnh
                if (uploadPhoto != null)
                {
                    string fileName = $"{DateTime.Now.Ticks}_{uploadPhoto.FileName}";
                    string folder = Path.Combine(ApplicationContext.WWWRootPath, @"images\categories");

                    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                    string filePath = Path.Combine(folder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await uploadPhoto.CopyToAsync(stream);
                    }
                    data.Photo = fileName;
                }

                if (!ModelState.IsValid)
                {
                    return View("Edit", data);
                }

                if (data.CategoryID == 0)
                    await CommonDataService.CategoryDB.AddAsync(data);
                else
                    await CommonDataService.CategoryDB.UpdateAsync(data);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Edit", data);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id = 0)
        {
            var model = await CommonDataService.CategoryDB.GetAsync(id);
            if (model == null)
                return RedirectToAction("Index");

            bool inUse = await CommonDataService.CategoryDB.InUsedAsync(id);
            if (inUse)
            {
                ViewBag.ErrorMessage = "Không thể xóa loại hàng này vì đang được sử dụng.";
            }

            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirm(int id)
        {
            bool inUse = await CommonDataService.CategoryDB.InUsedAsync(id);
            if (inUse)
            {
                TempData["ErrorMessage"] = "Không thể xóa loại hàng này vì đang được sử dụng.";
                return RedirectToAction("Index");
            }

            await CommonDataService.CategoryDB.DeleteAsync(id);
            TempData["Message"] = "Xóa loại hàng thành công!";
            return RedirectToAction("Index");
        }
    }
}