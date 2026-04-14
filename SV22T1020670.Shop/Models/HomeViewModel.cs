using SV22T1020670.DomainModels;

namespace SV22T1020670.Shop.Models
{
    public class HomeViewModel
    {
        public List<Advertisement> Banners { get; set; } = new List<Advertisement>();
        public List<Category> Categories { get; set; } = new List<Category>();
        public List<Product> FeaturedProducts { get; set; } = new List<Product>();
    }
}