using System;
using System.Collections.Generic;

namespace Stott.Optimizely.RobotsHandler.Presentation.ViewModels;

public sealed class SiteViewModel
{
    public Guid SiteId { get; set; }

    public string SiteName { get; set; }

    public List<SiteHostViewModel> AvailableHosts { get; set; }
}