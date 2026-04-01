namespace OptimizelyTwelveTest.Features.Home;

using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using OptimizelyTwelveTest.Features.Common;

public class HomePageController : PageControllerBase<HomePage>
{
    public async Task<IActionResult> Index(HomePage currentPage)
    {
        var model = new HomePageViewModel { CurrentPage = currentPage };

        return View(model);
    }
}
