using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020670.Admin.AppCodes;
using SV22T1020670.BusinessLayers;
using SV22T1020670.Admin;
using System.Threading.Tasks;
using SV22T1020670.DomainModels;
using SV22T1020670.DomainModels.Models;

namespace SV22T1020670.Admin.Controllers
{
    [Authorize]
    public class ShipperController : Controller
    {
        public const int PAGESIZE = 10;
        private const string SHIPPER_SEARCH_CONDITION = "ShipperSearchCondition";

        /// <summary>
        /// Hiển thị giao diện, nạp điều kiện tìm kiếm từ Session
        /// </summary>
        public IActionResult Index()
        {
            var condition = ApplicationContext.GetSessionData<PaginationSearchInput>(SHIPPER_SEARCH_CONDITION);
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

        /// <summary>
        /// Hàm Search trả về PartialView (AJAX)
        /// </summary>
        public async Task<IActionResult> Search(PaginationSearchInput condition)
        {
            // Lưu lại điều kiện vào Session
            ApplicationContext.SetSessionData(SHIPPER_SEARCH_CONDITION, condition);

            var data = await CommonDataService.ShipperDB.ListAsync(condition.Page, condition.PageSize, condition.SearchValue);
            var rowCount = await CommonDataService.ShipperDB.CountAsync(condition.SearchValue);

            var model = new SV22T1020670.DomainModels.PaginationSearchResult<Shipper>()
            {
                Page = condition.Page,
                PageSize = condition.PageSize,
                SearchValue = condition.SearchValue,
                RowCount = rowCount,
                Data = data
            };

            // Trả về PartialView
            return PartialView(model);
        }

        /// <summary>
        /// mở form tạo nhân viên giao hàng mới
        /// </summary>
        public IActionResult Create()
        {
            ViewBag.Title = "Thêm nhân viên giao hàng";
            var model = new Shipper()
            {
                ShipperID = 0
            };
            return View("Edit", model);
        }

        /// <summary>
        /// Mở form chỉnh sửa
        /// </summary>
        public async Task<IActionResult> Edit(int id = 0)
        {
            ViewBag.Title = "Chỉnh sửa thông tin nhân viên giao hàng";
            var model = await CommonDataService.ShipperDB.GetAsync(id);
            if (model == null)
                return RedirectToAction("Index");
            return View(model);
        }

        /// <summary>
        /// Lưu dữ liệu (Thêm/Cập nhật)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SaveData(Shipper data)
        {
            try
            {
                ViewBag.Title = data.ShipperID == 0 ? "Thêm nhân viên giao hàng" : "Chỉnh sửa thông tin nhân viên giao hàng";

                // Thêm kiểm tra dữ liệu
                if (string.IsNullOrWhiteSpace(data.ShipperName))
                    ModelState.AddModelError(nameof(data.ShipperName), "Tên không được để trống");
                if (string.IsNullOrWhiteSpace(data.Phone))
                    ModelState.AddModelError(nameof(data.Phone), "Số điện thoại không được để trống");

                if (!ModelState.IsValid)
                    return View("Edit", data);

                if (data.ShipperID == 0)
                    await CommonDataService.ShipperDB.AddAsync(data);
                else
                    await CommonDataService.ShipperDB.UpdateAsync(data);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Edit", data);
            }
        }

        /// <summary>
        /// Hiển thị form xác nhận Xóa (GET)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Delete(int id = 0)
        {
            var model = await CommonDataService.ShipperDB.GetAsync(id);
            if (model == null)
                return RedirectToAction("Index");

            bool inUse = await CommonDataService.ShipperDB.InUsedAsync(id);
            if (inUse)
            {
                ViewBag.ErrorMessage = "Không thể xóa người giao hàng này vì đang được sử dụng.";
            }

            return View(model);
        }

        /// <summary>
        /// Xử lý Xóa (POST)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> DeleteConfirm(int id)
        {
            bool inUse = await CommonDataService.ShipperDB.InUsedAsync(id);
            if (inUse)
            {
                TempData["ErrorMessage"] = "Không thể xóa người giao hàng này vì đang được sử dụng.";
                return RedirectToAction("Index");
            }

            await CommonDataService.ShipperDB.DeleteAsync(id);
            TempData["Message"] = "Xóa người giao hàng thành công!";
            return RedirectToAction("Index");
        }
    }
}
