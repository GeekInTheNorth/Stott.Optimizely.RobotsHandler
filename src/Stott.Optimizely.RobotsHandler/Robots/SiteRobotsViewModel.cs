using System;
using System.Collections.Generic;

using Stott.Optimizely.RobotsHandler.Common;
using Stott.Optimizely.RobotsHandler.Sites;

namespace Stott.Optimizely.RobotsHandler.Robots;

public sealed class SiteRobotsViewModel : ISiteContentViewModel
{
    public Guid Id { get; set; }

    public Guid SiteId { get; set; }

    public string SiteName { get; set; }

    public List<SiteHostViewModel> AvailableHosts { get; set; }

    public bool IsForWholeSite { get; set; }

    public string SpecificHost { get; set; }

    public string RobotsContent { get; set; }

    public bool CanDelete { get; set; }
}
