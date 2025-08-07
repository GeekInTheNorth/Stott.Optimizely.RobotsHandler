using System;

namespace Stott.Optimizely.RobotsHandler.Llms;

public sealed class SaveLlmsModel
{
    public Guid Id { get; set; }

    public Guid SiteId { get; set; }

    public string SiteName { get; set; }

    public string SpecificHost { get; set; }

    public string LlmsContent { get; set; }
}
