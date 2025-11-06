using System.Collections.Generic;

namespace Stott.Optimizely.RobotsHandler.Content.Models;

public sealed class ContentMarkdownSettingsDto
{
    public bool IsEnabled { get; set; }

    public IList<string> UserAgents { get; set; }
}