using System;

namespace Stott.Optimizely.RobotsHandler.Opal.Models;

public class SiteLlmsOpalModel
{
    public Guid Id { get; set; }

    public string SiteName { get; set; }

    public bool IsDefaultForSite { get; set; }

    public string SpecificHost { get; set; }

    public string LlmsContent { get; set; }
}