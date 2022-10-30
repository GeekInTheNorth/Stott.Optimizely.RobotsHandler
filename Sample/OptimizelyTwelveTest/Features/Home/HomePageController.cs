namespace OptimizelyTwelveTest.Features.Home
{
    using Microsoft.AspNetCore.Mvc;

    using OptimizelyTwelveTest.Features.Common;


    public class HomePageController : PageControllerBase<HomePage>
    {
        public IActionResult Index(HomePage currentPage)
        {
            var model = new HomePageViewModel { CurrentPage = currentPage };

            return View(model);
        }
    }
}
