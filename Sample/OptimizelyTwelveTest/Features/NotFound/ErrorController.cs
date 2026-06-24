namespace OptimizelyTwelveTest.Features.NotFound;

using EPiServer;

using Microsoft.AspNetCore.Mvc;

using OptimizelyTwelveTest.Features.Settings;

public sealed class ErrorController : Controller
{
    private readonly IContentLoader _contentLoader;
    private readonly ISiteSettingsResolver _siteSettingsResolver;

    public ErrorController(
        IContentLoader contentLoader,
        ISiteSettingsResolver siteSettingsResolver)
    {
        _contentLoader = contentLoader;
        _siteSettingsResolver = siteSettingsResolver;
    }

    [HttpGet]
    [Route("/error")]
    public IActionResult PageNotFound(int statusCode)
    {
        var notFoundPage = GetNotFoundPage();
        var model = new NotFoundPageViewModel() { CurrentPage = notFoundPage, StatusCode = statusCode };

        return View(model);
    }

    private NotFoundPage GetNotFoundPage()
    {
        var siteSettings = _siteSettingsResolver.Get();
        if (_contentLoader.TryGet<NotFoundPage>(siteSettings?.NotFoundPage, out var notFoundPage))
        {
            return notFoundPage;
        }

        return default;
    }
}