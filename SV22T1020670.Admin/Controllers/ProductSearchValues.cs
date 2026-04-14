using SV22T1020670.Admin    ;
using SV22T1020670.DomainModels;

namespace SV22T1020670.Admin.Controllers
{
    public class ProductSearchResult
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string SearchValues { get; set; } = "";
        public int CategoryId { get; set; }
        public int SupplierId { get; set; }
        public decimal MaxPrice { get; set; }
        public decimal MinPrice { get; set; }
        public IEnumerable<Product> Data { get; set; } = new List<Product>();
        public int RowCount { get; internal set; }
        
        /// <summary>
        /// Số trang
        /// </summary>
        public int PageCount 
        { 
            get
            {
                if (PageSize <= 0)
                    return 1;
                int p = RowCount / PageSize;
                if (RowCount % PageSize > 0)
                    p += 1;
                return p;
            }
        }
    }
}