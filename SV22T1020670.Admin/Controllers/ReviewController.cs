using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020670.Admin.AppCodes;
using SV22T1020670.BusinessLayers;
using SV22T1020670.DomainModels;
using SV22T1020670.DomainModels.Models;

namespace SV22T1020670.Admin.Controllers
{
    [Authorize(Roles = WebUserRoles.Administrator)]
    public class ReviewController : Controller
    {
        private const int PAGE_SIZE = 20;
        private const string REVIEW_SEARCH = "review_search";
        public IActionResult Index()
        {
            var input = ApplicationContext.GetSessionData<PaginationSearchInput>(REVIEW_SEARCH);
            if (input == null)
            {
                input = new PaginationSearchInput()
                {
                    Page = 1,
                    PageSize = PAGE_SIZE,
                    SearchValue = ""
                };
            }
            return View(input);
        }

        public IActionResult Search(PaginationSearchInput input)
        {
            int rowCount = 0;
            var data = ProductDataService.ProductDB.ListReviews(input.Page, input.PageSize, input.SearchValue, out rowCount);

            var model = new PaginationSearchResult<ProductReview>()
            {
                Page = input.Page,
                PageSize = input.PageSize,
                SearchValue = input.SearchValue,
                RowCount = rowCount,
                Data = data
            };

            ApplicationContext.SetSessionData(REVIEW_SEARCH, input);
            return PartialView("Search", model);
        }

        // Ẩn / hiện review
        public IActionResult Toggle(int id)
        {
            ProductDataService.ProductDB.ToggleReviewStatus(id);
            return RedirectToAction("Index");
        }

        // Xóa review
        public IActionResult Delete(int id)
        {
            ProductDataService.ProductDB.DeleteReview(id);
            return RedirectToAction("Index");
        }
    }
}
