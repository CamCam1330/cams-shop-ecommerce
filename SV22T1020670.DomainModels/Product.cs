using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV22T1020670.DomainModels
{
    /// <summary>
    /// Mặt hàng
    /// </summary>
    public class Product
    {
        /// <summary>
        /// Mã mặt hàng
        /// </summary>
        public int ProductID { get; set; }

        /// <summary>
        /// Tên mặt hàng
        /// </summary>
        public string ProductName { get; set; } = "";

        /// <summary>
        /// Mô tả thông tin về mặt hàng
        /// </summary>
        public string ProductDescription { get; set; } = "";

        /// <summary>
        /// Mã nhà cung cấp
        /// </summary>
        public int SupplierID { get; set; }

        /// <summary>
        /// Mã loại hàng
        /// </summary>
        public int CategoryID { get; set; }

        /// <summary>
        /// Đơn vị tính
        /// </summary>
        public string Unit { get; set; } = "";
        /// <summary>
        /// Giá mặt hàng
        /// </summary>
        
        ///<summary>
        /// Số lượng
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Giá bán 
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Giá sale up
        /// </summary>
        public decimal SalePrice { get; set; }

        /// <summary>
        /// Tên file ảnh
        /// </summary>
        public string Photo { get; set; } = "";

        /// <summary>
        /// Mặt hàng có đang được bán hay không?
        /// </summary>
        public bool IsSelling { get; set; }
    }
}
