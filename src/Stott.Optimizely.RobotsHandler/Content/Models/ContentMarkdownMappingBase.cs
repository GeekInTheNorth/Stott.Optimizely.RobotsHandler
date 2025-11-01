using System;

namespace Stott.Optimizely.RobotsHandler.Content.Models;

public abstract class ContentMarkdownMappingBase
{
    public Guid Id { get; set; }

    public string DisplayName { get; set; }

    public string Description { get; set; }

    public string ContentName { get; set; }

    public string ContentType { get; set; }

    public bool IsEnabled { get; set; }

    public bool IsConfigured { get; set; }
}
