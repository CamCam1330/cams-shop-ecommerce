using SV22T1020670.DomainModels;

namespace SV22T1020670.Shop.Models
{
    public class ProductSearchOutput : PaginationSearchResult<Product>
    {
        public int CategoryID { get; set; }
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }

        public string SortBy { get; set; } = "";
    }
}
