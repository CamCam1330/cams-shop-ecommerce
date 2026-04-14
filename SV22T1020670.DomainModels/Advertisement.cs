using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV22T1020670.DomainModels
{
    public class Advertisement
    {
        /// <summary>
        /// ID của bảng quảng cáo
        /// </summary>
        public int BannerID { get; set; }
        /// <summary>
        /// Tiêu đề
        /// </summary>
        public string Title { get; set; } = "";
        /// <summary>
        /// Ảnh minh họa
        /// </summary>
        public string Photo { get; set; } = "";
        /// <summary>
        /// 
        /// </summary>
        public string Link { get; set; } = "";
        public int DisplayOrder { get; set; }
        public bool IsHidden { get; set; }
    }
}
