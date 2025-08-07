﻿using System;
using System.Collections.Generic;

namespace Stott.Optimizely.RobotsHandler.Sites;

public sealed class SiteViewModel
{
    public Guid SiteId { get; set; }

    public string SiteName { get; set; }

    public List<SiteHostViewModel> AvailableHosts { get; set; }
}