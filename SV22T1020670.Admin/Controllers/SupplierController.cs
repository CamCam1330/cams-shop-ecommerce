using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020670.BusinessLayers;
using SV22T1020670.Admin;
using System;
using System.Threading.Tasks;
using SV22T1020670.Admin.AppCodes;
using SV22T1020670.Admin.Models;
using SV22T1020670.DomainModels.Models;
using SV22T1020670.DomainModels;

namespace SV22T1020670.Admin.Controllers
{
    [Authorize]
    public class SupplierController : Controller
    {
        public const int PAGESIZE = 18; // Giữ nguyên PageSize của Supplier
        private const string SUPPLIER_SEARCH_CONDITION = "SupplierSearchCondition";


        /// <summary>
        /// Hiển thị danh sách Nhà cung cấp
        /// </summary>
        public IActionResult Index()
        {
            /// Lấy điều kiện tìm kiếm từ Session
            var condition = ApplicationContext.GetSessionData<PaginationSearchInput>(SUPPLIER_SEARCH_CONDITION);

            /// Nếu không có, tạo điều kiện mặc định
            if (condition == null)
            {
                condition = new PaginationSearchInput()
                {
                    Page = 1,
                    PageSize = PAGESIZE,
                    SearchValue = ""
                };
            }

            // Trả về View với model là điều kiện tìm kiếm (giống Customer)
            return View(condition);
        }

        /// <summary>
        /// Hàm xử lý AJAX (giống Customer)
        /// </summary>
        public async Task<IActionResult> Search(PaginationSearchInput condition)
        {
            // Lưu lại điều kiện tìm kiếm vào Session
            ApplicationContext.SetSessionData(SUPPLIER_SEARCH_CONDITION, condition);

            var data = await CommonDataService.SupplierDB.ListAsync(condition.Page, condition.PageSize, condition.SearchValue);
            var rowCount = await CommonDataService.SupplierDB.CountAsync(condition.SearchValue);

            var model = new DomainModels.PaginationSearchResult<Supplier>()
            {
                Page = condition.Page,
                PageSize = condition.PageSize,
                SearchValue = condition.SearchValue,
                RowCount = rowCount,
                Data = data
            };

            // Trả về PartialView (quan trọng)
            return PartialView(model);
        }

        /// <summary>
        /// mở form tạo nhà cung cấp mới
        /// </summary>
        /// <returns></returns>
        public IActionResult Create()
        {
            ViewBag.Title = "Bổ sung nhà cung cấp";
            var model = new Supplier()
            {
                SupplierID = 0
            };
            return View("Edit", model);
        }

        /// <summary>
        /// mở form chỉnh sửa thông tin nhà cung cấp
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Edit(int id = 0)
        {
            ViewBag.Title = "Cập nhật thông tin nhà cung cấp";
            var model = await CommonDataService.SupplierDB.GetAsync(id);
            if (model == null)
                return RedirectToAction("Index");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveData(Supplier data)
        {
            // Bổ sung try-catch để bắt lỗi
            try
            {
                // Bổ sung kiểm tra dữ liệu đầu vào (Validation)
                ViewBag.Title = data.SupplierID == 0 ? "Bổ sung nhà cung cấp" : "Cập nhật thông tin nhà cung cấp";

                if (string.IsNullOrWhiteSpace(data.SupplierName))
                    ModelState.AddModelError(nameof(data.SupplierName), "Tên nhà cung cấp không được để trống");
                if (string.IsNullOrWhiteSpace(data.ContactName))
                    ModelState.AddModelError(nameof(data.ContactName), "Tên liên hệ không được để trống");
                if (string.IsNullOrWhiteSpace(data.Phone))
                    ModelState.AddModelError(nameof(data.Phone), "Số điện thoại không được để trống");
                if (string.IsNullOrWhiteSpace(data.Email))
                    ModelState.AddModelError(nameof(data.Email), "Email không được để trống");
                if (string.IsNullOrWhiteSpace(data.Address))
                    ModelState.AddModelError(nameof(data.Address), "Địa chỉ không được để trống");
                if (string.IsNullOrWhiteSpace(data.Province))
                    ModelState.AddModelError(nameof(data.Province), "Tỉnh/Thành phố không được để trống");

                // Nếu có lỗi, trả về View("Edit")
                if (!ModelState.IsValid)
                {
                    return View("Edit", data);
                }

                if (data.SupplierID == 0)
                    await CommonDataService.SupplierDB.AddAsync(data);
                else
                    await CommonDataService.SupplierDB.UpdateAsync(data);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Báo lỗi nếu có exception
                ModelState.AddModelError("", ex.Message);
                return View("Edit", data);
            }
        }

        /// <summary>
        /// Hiển thị trang xác nhận xóa nhà cung cấp
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Delete(int id = 0)
        {
            var model = await CommonDataService.SupplierDB.GetAsync(id);
            if (model == null)
                return RedirectToAction("Index");

            // Bổ sung kiểm tra InUsed (giả định có phương thức này)
            bool inUse = await CommonDataService.SupplierDB.InUsedAsync(id);
            if (inUse)
            {
                ViewBag.ErrorMessage = "Không thể xóa nhà cung cấp này vì đang được sử dụng.";
            }

            return View(model); // Trả về view "Delete.cshtml"
        }

        /// <summary>
        /// Xử lý xóa nhà cung cấp (sau khi xác nhận)
        /// </summary>
        [HttpPost] // Tách thành [HttpPost]
        public async Task<IActionResult> DeleteConfirm(int id)
        {
            // Kiểm tra lại InUsed trước khi xóa
            bool inUse = await CommonDataService.SupplierDB.InUsedAsync(id);
            if (inUse)
            {
                TempData["ErrorMessage"] = "Không thể xóa nhà cung cấp này vì đang được sử dụng.";
                return RedirectToAction("Index");
            }

            await CommonDataService.SupplierDB.DeleteAsync(id);
            TempData["Message"] = "Xóa nhà cung cấp thành công!";
            return RedirectToAction("Index");
        }
    }
}
