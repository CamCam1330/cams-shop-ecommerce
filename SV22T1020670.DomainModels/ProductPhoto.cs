using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV22T1020670.DomainModels
{
    /// <summary>
    /// Ảnh của mặt hàng
    /// </summary>
    public class ProductPhoto
    {
        /// <summary>
        /// Mã ảnh
        /// </summary>
        public long PhotoID { get; set; }
        /// <summary>
        /// Mã mặt hàng
        /// </summary>
        public int ProductID { get; set; }
        /// <summary>
        /// Tên file ảnh
        /// </summary>
        public string Photo { get; set; } = "";
        /// <summary>
        /// Mô tả/thuyết minh ảnh
        /// </summary>
        public string Description { get; set; } = "";
        /// <summary>
        /// Thứ tự hiển thị
        /// </summary>
        public int DisplayOrder { get; set; }
        /// <summary>
        /// Ảnh có bị ẩn hay không?
        /// </summary>
        public bool IsHidden { get; set; }
    }
}
