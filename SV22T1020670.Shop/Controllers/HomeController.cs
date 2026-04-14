using Microsoft.AspNetCore.Mvc;
using SV22T1020670.BusinessLayers;
using SV22T1020670.Shop.Models;
using System.Diagnostics;

namespace SV22T1020670.Shop.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            // 1. Khởi tạo ViewModel 
            var model = new HomeViewModel();

            // 2. Lấy dữ liệu Banner (Quảng cáo)
            var listBanners = await AdvertisementDataService.AdvertisementDB.ListAsync(1, 20, "");
            model.Banners = listBanners.Where(x => !x.IsHidden).OrderBy(x => x.DisplayOrder).ToList();

            // 3. Lấy dữ liệu Danh mục (Categories) 
            var listCategories = await CommonDataService.CategoryDB.ListAsync(1, 20, "");
            model.Categories = listCategories.ToList();

            // 4. Lấy dữ liệu Sản phẩm (Products) 
            var listProducts = await CommonDataService.ProductDB.ListAsync(1, 8, "");
            model.FeaturedProducts = listProducts.ToList();

            return View(model);
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}