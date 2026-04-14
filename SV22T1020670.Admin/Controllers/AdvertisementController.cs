using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020670.BusinessLayers;
using SV22T1020670.DomainModels;
using System.Buffers;
using SV22T1020670.Admin.AppCodes;
using SV22T1020670.DomainModels.Models;

namespace SV22T1020670.Admin.Controllers
{
    [Authorize(Roles = "admin")]
    public class AdvertisementController : Controller
    {
        private const int PAGE_SIZE = 20;
        private const string SEARCH_CONDITION = "AdvertisementSearchCondition";

        public IActionResult Index()
        {
            PaginationSearchInput? condition = ApplicationContext.GetSessionData<PaginationSearchInput>(SEARCH_CONDITION);
            if (condition == null)
            {
                condition = new PaginationSearchInput()
                {
                    Page = 1,
                    PageSize = PAGE_SIZE,
                    SearchValue = ""
                };
            }
            return View(condition);
        }

        public async Task<IActionResult> Search(PaginationSearchInput condition)
        {
            int rowCount = await AdvertisementDataService.AdvertisementDB.CountAsync(condition.SearchValue ?? "");
            var data = await AdvertisementDataService.AdvertisementDB.ListAsync(condition.Page, condition.PageSize, condition.SearchValue ?? "");

            var model = new PaginationSearchResult<Advertisement>()
            {
                Page = condition.Page,
                PageSize = condition.PageSize,
                SearchValue = condition.SearchValue ?? "",
                RowCount = rowCount,
                Data = data
            };

            ApplicationContext.SetSessionData(SEARCH_CONDITION, condition);

            return View(model);
        }

        public IActionResult Create()
        {
            var model = new Advertisement()
            {
                BannerID = 0,
                DisplayOrder = 1,
                IsHidden = false,
                Photo = "nophoto.png" // Giá trị mặc định
            };
            return View("Edit", model);
        }

        public async Task<IActionResult> Edit(int id = 0)
        {
            var model = await AdvertisementDataService.AdvertisementDB.GetAsync(id);
            if (model == null)
                return RedirectToAction("Index");

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Save(Advertisement data, IFormFile? uploadPhoto)
        {
            try
            {
                // 1. Validate dữ liệu
                if (string.IsNullOrWhiteSpace(data.Title))
                    ModelState.AddModelError(nameof(data.Title), "Vui lòng nhập tiêu đề quảng cáo");

                // Nếu là thêm mới thì bắt buộc phải chọn ảnh
                if (data.BannerID == 0 && uploadPhoto == null)
                    ModelState.AddModelError(nameof(data.Photo), "Vui lòng chọn hình ảnh quảng cáo");

                // 2. Xử lý Upload ảnh
                if (uploadPhoto != null)
                {
                    // Tên file = TimeStamp + Tên gốc (Tránh trùng lặp)
                    string fileName = $"{DateTime.Now.Ticks}_{uploadPhoto.FileName}";

                    // Đường dẫn lưu ảnh: wwwroot/images/banners
                    string folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "banners");

                    // Tạo thư mục nếu chưa có
                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);

                    string filePath = Path.Combine(folder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await uploadPhoto.CopyToAsync(stream);
                    }
                    data.Photo = fileName; // Cập nhật tên ảnh mới vào Model
                }

                if (!ModelState.IsValid)
                {
                    return View("Edit", data);
                }

                // 3. Gọi hàm xử lý Business
                if (data.BannerID == 0)
                {
                    await AdvertisementDataService.AdvertisementDB.AddAsync(data);
                }
                else
                {
                    await AdvertisementDataService.AdvertisementDB.UpdateAsync(data);
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Error", "Lỗi hệ thống: " + ex.Message);
                return View("Edit", data);
            }
        }

        public async Task<IActionResult> Delete(int id = 0)
        {
            if (Request.Method == "POST")
            {
                await AdvertisementDataService.AdvertisementDB.DeleteAsync(id);
                return RedirectToAction("Index");
            }

            var model = await AdvertisementDataService.AdvertisementDB.GetAsync(id);
            if (model == null)
                return RedirectToAction("Index");

            return View(model);
        }
    }
}