using System;
using System.Collections.Generic;

namespace Stott.Optimizely.RobotsHandler.Services;

public sealed class SiteRobotsViewModel
{
    public Guid Id { get; set; }

    public Guid SiteId { get; set; }

    public string SiteName { get; set; }

    public List<KeyValuePair<string, string>> AvailableHosts { get; set; }

    public bool IsForWholeSite { get; set; }

    public string SpecificHost { get; set; }

    public string RobotsContent { get; set; }
}
