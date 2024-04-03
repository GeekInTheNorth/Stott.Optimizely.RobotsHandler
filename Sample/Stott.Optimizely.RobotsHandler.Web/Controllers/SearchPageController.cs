using Microsoft.AspNetCore.Mvc;
using Stott.Optimizely.RobotsHandler.Web.Models.Pages;
using Stott.Optimizely.RobotsHandler.Web.Models.ViewModels;

namespace Stott.Optimizely.RobotsHandler.Web.Controllers
{
    public class SearchPageController : PageControllerBase<SearchPage>
    {
        public ViewResult Index(SearchPage currentPage, string q)
        {
            var model = new SearchContentModel(currentPage)
            {
                Hits = Enumerable.Empty<SearchContentModel.SearchHit>(),
                NumberOfHits = 0,
                SearchServiceDisabled = true,
                SearchedQuery = q
            };

            return View(model);
        }
    }
}
