using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV22T1020670.DomainModels
{
    /// <summary>
    /// Thông tin đơn vị giao hàng (shipper)
    /// </summary>
    public class Shipper
    {
        /// <summary>
        /// Mã đơn vị giao hàng
        /// </summary>
        public int ShipperID { get; set; }

        /// <summary>
        /// Tên đơn vị giao hàng hoặc tên người giao
        /// </summary>
        [Required(ErrorMessage = "Tên người giao hàng không được để trống")]
        [StringLength(50, ErrorMessage = "Tên không quá 50 ký tự")]
        public string ShipperName { get; set; } = "";

        /// <summary>
        /// Số điện thoại liên hệ của đơn vị giao hàng
        /// </summary>
        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [RegularExpression(@"^0\d{9,10}$", ErrorMessage = "Số điện thoại không hợp lệ (phải bắt đầu bằng 0 và có 10-11 số)")]
        public string Phone { get; set; } = "";
    }
}
