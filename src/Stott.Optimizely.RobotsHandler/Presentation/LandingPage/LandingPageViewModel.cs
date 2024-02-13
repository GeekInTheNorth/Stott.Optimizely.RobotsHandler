namespace Stott.Optimizely.RobotsHandler.Presentation.LandingPage;

public sealed class LandingPageViewModel
{
    public string ApplicationName { get; internal set; }

    public string ApplicationVersion { get; internal set; }

    public string JavaScriptFile { get; internal set; }

    public string CssFile { get; internal set; }

    public string Nonce { get; internal set; }
}