using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV22T1020670.DomainModels
{
    /// <summary>
    /// Thông tin nhân viên
    /// </summary>
    public class Employee
    {
        /// <summary>
        /// Mã nhân viên
        /// </summary>
        public int EmployeeID { get; set; }

        /// <summary>
        /// Họ và tên nhân viên
        /// </summary>
        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(50, ErrorMessage = "Họ tên không quá 50 ký tự")]
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Ngày sinh của nhân viên
        /// </summary>
        public DateTime? BirthDate { get; set; }

        /// <summary>
        /// Địa chỉ nơi cư trú
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// Số điện thoại liên hệ
        /// </summary>
        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        public string Phone { get; set; } = "";

        /// <summary>
        /// Địa chỉ email của nhân viên
        /// </summary>
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = "";

        /// <summary>
        /// Mật khẩu đăng nhập của nhân viên
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// Đường dẫn ảnh đại diện (nếu có)
        /// </summary>
        public string? Photo { get; set; }

        /// <summary>
        /// Trạng thái làm việc (true = đang làm, false = nghỉ)
        /// </summary>
        public bool IsWorking { get; set; }
    }
}
