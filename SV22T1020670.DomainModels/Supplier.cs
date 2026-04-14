using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV22T1020670.DomainModels
{

    public class Supplier
    {
        /// <summary>
        /// Mã nhà cung cấp
        /// </summary>
        public int SupplierID { get; set; }

        /// <summary>
        /// Tên công ty hoặc tên nhà cung cấp
        /// </summary>
        [Required(ErrorMessage = "Tên nhà cung cấp không được để trống")]
        public string SupplierName { get; set; } = "";

        /// <summary>
        /// Tên người liên hệ của nhà cung cấp
        /// </summary>
        [Required(ErrorMessage = "Tên giao dịch không được để trống")]
        public string ContactName { get; set; } = "";

        /// <summary>
        /// Tỉnh/Thành phố nơi đặt trụ sở
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn Tỉnh/Thành")]
        public string Province { get; set; } = "";

        /// <summary>
        /// Địa chỉ chi tiết của nhà cung cấp
        /// </summary>
        public string Address { get; set; } = "";

        /// <summary>
        /// Số điện thoại liên hệ
        /// </summary>
        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [RegularExpression(@"^0\d{9,10}$", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Phone { get; set; } = "";

        /// <summary>
        /// Địa chỉ email của nhà cung cấp
        /// </summary>
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; } = "";
    }
}
