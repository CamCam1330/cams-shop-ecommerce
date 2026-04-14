using Microsoft.AspNetCore.Mvc.Rendering;
using SV22T1020670.BusinessLayers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SV22T1020670.Shop.AppCodes
{
    public static class SelectListHelper
    {
        /// <summary>
        /// Danh sách các tỉnh thành dùng cho thẻ select
        /// </summary>
        /// <returns></returns>
        public static async Task<IEnumerable<SelectListItem>> Provinces()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list.Add(new SelectListItem() { Value = "", Text = "-- Chọn Tỉnh/Thành --" });
            foreach (var item in await CommonDataService.ProvinceDB.ListAsync())
            {
                list.Add(new SelectListItem() { Value = item.ProvinceName, Text = item.ProvinceName });
            }
            return list;
        }

        public async static Task<IEnumerable<SelectListItem>> Customers()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list.Add(new SelectListItem() { Value = "", Text = "-- Khách hàng --" });
            foreach (var item in await CommonDataService.CustomerDB.ListAsync())
            {
                list.Add(new SelectListItem() { Value = item.CustomerID.ToString(), Text = item.CustomerName });
            }
            return list;
        }
    }
}