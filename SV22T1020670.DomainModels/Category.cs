using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV22T1020670.DomainModels
{
    /// <summary>
    /// Danh mục
    /// </summary>
    public class Category
    {
        /// <summary>
        /// Mã danh mục
        /// </summary>
        public int CategoryID { get; set; }
        /// <summary>
        /// Tên danh mục
        /// </summary>
        [Required(ErrorMessage = "Tên loại hàng không được để trống")]
        [StringLength(50, ErrorMessage = "Tên loại hàng không quá 50 ký tự")]
        public string CategoryName { get; set; } = "";

        /// <summary>
        /// Mô tả danh mục
        /// </summary>
        [StringLength(255, ErrorMessage = "Mô tả không quá 255 ký tự")]
        public string Description { get; set; } = "";

        /// <summary>
        /// Ảnh 
        /// </summary>
        public string Photo { get; set; } = "";
    }
}
