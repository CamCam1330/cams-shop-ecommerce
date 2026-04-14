using System.ComponentModel.DataAnnotations;

namespace SV22T1020670.DomainModels
{
    public class Customer
    {
        /// <summary>
        /// Mã Khách hàng
        /// </summary>
        public int CustomerID { get; set; }
        /// <summary>
        /// Tên khách hàng
        /// </summary>
        [Required(ErrorMessage = "Tên khách hàng không được để trống")]
        [StringLength(50, ErrorMessage = "Tên khách hàng không quá 50 ký tự")]
        public string CustomerName { get; set; } = "";

        /// <summary>
        /// Tên giao dịch
        /// </summary>
        [Required(ErrorMessage = "Tên giao dịch không được để trống")]
        [StringLength(50, ErrorMessage = "Tên giao dịch không quá 50 ký tự")]
        public string ContactName { get; set; } = "";

        /// <summary>
        /// Tỉnh thành
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn Tỉnh/Thành")]
        public string Province { get; set; } = "";

        /// <summary>
        /// Địa chỉ
        /// </summary>
        [Required(ErrorMessage = "Địa chỉ không được để trống")]
        [StringLength(100, ErrorMessage = "Địa chỉ không quá 100 ký tự")]
        public string Address { get; set; } = "";

        /// <summary>
        /// Số điện thoại
        /// </summary>
        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [RegularExpression(@"^0\d{9,10}$", ErrorMessage = "SĐT không hợp lệ (Bắt đầu bằng 0, dài 10-11 số)")]
        public string Phone { get; set; } = "";

        /// <summary>
        /// Email
        /// </summary>
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; } = "";

        /// <summary>
        /// Tài khoản khách có bị khóa hay không
        /// </summary>
        public bool IsLocked { get; set; }

        public string Password { get; set; } = "";
    }
}
