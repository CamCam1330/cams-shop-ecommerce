using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV22T1020670.DomainModels
{
    public class ProductReview
    {
        /// <summary>
        /// 
        /// </summary>
        public int ReviewID { get; set; }
        /// <summary>
        /// Mã sản phẩm
        /// </summary>
        public int ProductID { get; set; }
        /// <summary>
        /// ID của khách hàng
        /// </summary>
        public int CustomerID { get; set; }
        /// <summary>
        ///  Số sao đánh giá
        /// </summary>
        public int Rating { get; set; }
        /// <summary>
        /// Ý kiến của khách hàng
        /// </summary>
        public string Comment { get; set; } = "";
        /// <summary>
        /// Thời gian đánh giá
        /// </summary>
        public DateTime ReviewTime { get; set; }
        public bool IsHidden { get; set; }

        public string CustomerName { get; set; } = "";

        public string ProductName { get; set; }
    }
}
