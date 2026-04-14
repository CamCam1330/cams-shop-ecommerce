using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020670.DomainModels;
using SV22T1020670.Admin.AppCodes;
using SV22T1020670.BusinessLayers;
using System.IO;
using SV22T1020670.Admin.Models; // Cần cho xử lý file
using SV22T1020670.DomainModels.Models;

namespace SV22T1020670.Admin.Controllers
{
    [Authorize(Roles = $"{WebUserRoles.Administrator}")]
    public class EmployeeController : Controller
    {
        public const int PAGESIZE = 12; 
        private const string EMPLOYEE_SEARCH_CONDITION = "EmployeeSearchCondition";

        /// <summary>
        /// Hiển thị giao diện chính
        /// </summary>
        public IActionResult Index()
        {
            var condition = ApplicationContext.GetSessionData<PaginationSearchInput>(EMPLOYEE_SEARCH_CONDITION);
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
        /// Hàm Search AJAX trả về PartialView
        /// </summary>
        public async Task<IActionResult> Search(PaginationSearchInput condition)
        {
            ApplicationContext.SetSessionData(EMPLOYEE_SEARCH_CONDITION, condition);

            var data = await CommonDataService.EmployeeDB.ListAsync(condition.Page, condition.PageSize, condition.SearchValue);
            var rowCount = await CommonDataService.EmployeeDB.CountAsync(condition.SearchValue);

            var model = new PaginationSearchResult<Employee>()
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
            ViewBag.Title = "Bổ sung nhân viên";
            var model = new EmployeeEditModel()
            {
                EmployeeID = 0,
                Photo = "nophoto.png", // Ảnh mặc định
                IsWorking = true,
                BirthDate = new DateTime(1990, 1, 1) // Ngày sinh mặc định
            };
            return View("Edit", model);
        }

        public async Task<IActionResult> Edit(int id = 0)
        {
            ViewBag.Title = "Cập nhật thông tin nhân viên";
            var employee = await CommonDataService.EmployeeDB.GetAsync(id);
            if (employee == null)
                return RedirectToAction("Index");

            var model = new EmployeeEditModel()
            {
                EmployeeID = employee.EmployeeID,
                FullName = employee.FullName,
                BirthDate = employee.BirthDate,
                Address = employee.Address,
                Email = employee.Email,
                Phone = employee.Phone,
                Photo = employee.Photo,
                IsWorking = employee.IsWorking
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveData(EmployeeEditModel model)
        {
            try
            {
                ViewBag.Title = model.EmployeeID == 0 ? "Bổ sung nhân viên" : "Cập nhật thông tin nhân viên";

                // 1. Validation
                if (string.IsNullOrWhiteSpace(model.FullName))
                    ModelState.AddModelError(nameof(model.FullName), "Tên nhân viên không được để trống");
                if (string.IsNullOrWhiteSpace(model.Email))
                    ModelState.AddModelError(nameof(model.Email), "Email không được để trống");
                if (string.IsNullOrWhiteSpace(model.Phone))
                    ModelState.AddModelError(nameof(model.Phone), "Nhập số điện thoại của khách hàng");
                if (string.IsNullOrWhiteSpace(model.Address))
                ModelState.AddModelError(nameof(model.Address), "Địa chỉ không được để trống");

                // Nếu không hợp lệ thì trả về View để sửa
                if (!ModelState.IsValid)
                {
                    return View("Edit", model);
                }

                // 2. Xử lý ảnh upload
                if (model.UploadPhoto != null)
                {
                    // Tạo tên file: tick_tenfile.jpg để tránh trùng
                    string fileName = $"{DateTime.Now.Ticks}_{model.UploadPhoto.FileName}";

                    // Lưu vào thư mục wwwroot/images/employees
                    string filePath = Path.Combine(ApplicationContext.WWWRootPath, @"images\employees", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.UploadPhoto.CopyToAsync(stream);
                    }
                    model.Photo = fileName;
                }

                // 3. Map dữ liệu
                Employee data = new Employee()
                {
                    EmployeeID = model.EmployeeID,
                    FullName = model.FullName,
                    BirthDate = model.BirthDate,
                    Address = model.Address,
                    Email = model.Email,
                    Phone = model.Phone,
                    Photo = model.Photo,
                    IsWorking = model.IsWorking
                };

                // 4. Lưu DB
                if (data.EmployeeID == 0)
                    await CommonDataService.EmployeeDB.AddAsync(data);
                else
                    await CommonDataService.EmployeeDB.UpdateAsync(data);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi hệ thống: " + ex.Message);
                return View("Edit", model);
            }
        }

        // Tách Delete ra GET và POST cho an toàn
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var model = await CommonDataService.EmployeeDB.GetAsync(id);
            if (model == null) return RedirectToAction("Index");
            return View(model);
        }

        [HttpPost, ActionName("Delete")] // <--- THÊM DÒNG NÀY QUAN TRỌNG
        [ValidateAntiForgeryToken]       // <--- Nên thêm để bảo mật form
        public async Task<IActionResult> DeleteConfirm(int id)
        {
            // TODO: Kiểm tra InUsed nếu cần
            await CommonDataService.EmployeeDB.DeleteAsync(id);
            return RedirectToAction("Index");
        }
    }
}