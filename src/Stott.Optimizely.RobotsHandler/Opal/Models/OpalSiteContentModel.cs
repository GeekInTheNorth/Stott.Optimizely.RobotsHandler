using System;

namespace Stott.Optimizely.RobotsHandler.Opal.Models;

public class OpalSiteContentModel
{
    public Guid Id { get; set; }

    public string SiteName { get; set; }

    public string SpecificHost { get; set; }

    public string Content { get; set; }
}