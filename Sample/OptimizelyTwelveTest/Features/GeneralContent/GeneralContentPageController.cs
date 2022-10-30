namespace OptimizelyTwelveTest.Features.GeneralContent
{
    using Microsoft.AspNetCore.Mvc;

    using OptimizelyTwelveTest.Features.Common;

    public class GeneralContentPageController : PageControllerBase<GeneralContentPage>
    {
        public IActionResult Index(GeneralContentPage currentContent)
        {
            var model = new GeneralContentPageViewModel { CurrentPage = currentContent };

            return View(model);
        }
    }
}
