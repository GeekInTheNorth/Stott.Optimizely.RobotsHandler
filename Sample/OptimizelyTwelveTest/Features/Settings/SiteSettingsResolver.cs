using EPiServer;
using EPiServer.Core;

using OptimizelyTwelveTest.Features.Home;

namespace OptimizelyTwelveTest.Features.Settings;

public class SiteSettingsResolver : ISiteSettingsResolver
{
    private readonly IContentLoader _contentLoader;

    private ISiteSettings _siteSettings;

    public SiteSettingsResolver(IContentLoader contentLoader)
    {
        _contentLoader = contentLoader;
    }

    public ISiteSettings Get()
    {
        if (_siteSettings == null)
        {
            if (_contentLoader.TryGet<HomePage>(ContentReference.StartPage, out var homePage) &&
                _contentLoader.TryGet<SiteSettingsPage>(homePage.SiteSettings, out var siteSettingsPage))
            {
                _siteSettings = siteSettingsPage;
            }
        }

        return _siteSettings;
    }
}