
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SV22T1020670.Admin.AppCodes;
using SV22T1020670.BusinessLayers;
using SV22T1020670.Admin;
using System;
using System.Buffers;
using System.Linq.Expressions;
using System.Threading.Tasks;
using SV22T1020670.DomainModels;
using SV22T1020670.DomainModels.Models;

namespace SV22T1020670.Admin.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        private const int PAGESIZE = 20;
        private const string CUSTOMER_SEARCH_CONDITION = "CustomerSearchCondition";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="page"></param>
        /// <param name="searchValue"></param>
        /// <returns></returns>
        public IActionResult Index()
        {
            // Kiểm tra nếu trong Session có lưu điều kiện tìm kiếm thì sử dụng lại điều kiện đó
            // Ngược lại, thì tạo điều kiện tìm kiếm mặc định
            var condition = ApplicationContext.GetSessionData<PaginationSearchInput>(CUSTOMER_SEARCH_CONDITION);

            if (condition == null)
            {
                condition = new PaginationSearchInput
                {
                    Page = 1,
                    PageSize = PAGESIZE,
                    SearchValue = ""
                };
            }
            return View(condition);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public async Task<IActionResult> Search(PaginationSearchInput condition)
        {
            var data = await CommonDataService.CustomerDB.ListAsync(condition.Page, condition.PageSize, condition.SearchValue);
            var rowCount = await CommonDataService.CustomerDB.CountAsync(condition.SearchValue);

            var model = new PaginationSearchResult<Customer>()
            {
                Page = condition.Page,
                PageSize = condition.PageSize,
                SearchValue = condition.SearchValue ?? string.Empty,
                RowCount = rowCount,
                Data = data
            };

            //Lưu lại điều kiện tìm kiếm vào trong session
            ApplicationContext.SetSessionData(CUSTOMER_SEARCH_CONDITION, condition);

            return PartialView("Search", model);
        }

        /// <summary>
        /// Tạo mới khách hàng
        /// </summary>
        /// <returns></returns>
        public IActionResult Create()
        {
            ViewBag.Title = "Bổ sung khách hàng mới";
            var model = new Customer { CustomerID = 0 };
            return View("Edit", model);
        }

        /// <summary>
        /// Cập nhật thông tin khách hàng
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Edit(int id = 0)
        {
            ViewBag.Title = "Cập nhật thông tin khách hàng";
            var model = await CommonDataService.CustomerDB.GetAsync(id);
            if (model == null) return RedirectToAction("Index");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveData(Customer data)
        {
            try
            { ViewBag.Title = data.CustomerID == 0 ? "Bổ sung khách hàng mới" : "Cập nhật thông tin khách hàng";
                // Kiểm tra dữ liệu đầu vào
                if (string.IsNullOrWhiteSpace(data.CustomerName))
                    // bắt lỗi trên View dùng ModelState
                    ModelState.AddModelError(nameof(data.CustomerName), "Tên khách hàng không được để trống");
                if (string.IsNullOrWhiteSpace(data.ContactName))
                    ModelState.AddModelError(nameof(data.ContactName), "Nhập tên giao dịch của khách hàng");
                if (string.IsNullOrWhiteSpace(data.Phone))
                    ModelState.AddModelError(nameof(data.Phone), "Nhập số điện thoại của khách hàng");
                if (string.IsNullOrWhiteSpace(data.Email))
                    ModelState.AddModelError(nameof(data.Email), "Nhập email của khách hàng");
                if (string.IsNullOrWhiteSpace(data.Address))
                    ModelState.AddModelError(nameof(data.Address), "Nhập địa chỉ của khách hàng");
                if (string.IsNullOrWhiteSpace(data.Province))
                    ModelState.AddModelError(nameof(data.Province), "Chọn tỉnh/thành");

                //Thông báo lỗi và yêu cầu nhập lại dữ liệu nếu có trường hợp không hợp lệ
                if (!ModelState.IsValid)
                    return View("Edit", data);

                if (data.CustomerID == 0)
                    await CommonDataService.CustomerDB.AddAsync(data);
                else
                    await CommonDataService.CustomerDB.UpdateAsync(data);

                return RedirectToAction("Index");
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("Error", ex.Message);
                return View("Edit",data);
            }
        }

        public async Task<IActionResult> Delete(int id = 0)
        {
            if (Request.Method == "POST")
            {
                await CommonDataService.CustomerDB.DeleteAsync(id);
                return RedirectToAction("Index");
            }

            var model = await CommonDataService.CustomerDB.GetAsync(id);
            if (model == null) return RedirectToAction("Index");
            return View(model);
        }
    }
}
