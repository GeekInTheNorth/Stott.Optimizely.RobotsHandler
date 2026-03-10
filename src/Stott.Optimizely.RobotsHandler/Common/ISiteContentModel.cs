using System;
using System.Collections.Generic;

using Stott.Optimizely.RobotsHandler.Applications;

namespace Stott.Optimizely.RobotsHandler.Common;

public interface IApplicationContentViewModel
{
    public Guid Id { get; set; }

    public string? AppId { get; set; }

    public string? AppName { get; set; }

    public List<HostViewModel> AvailableHosts { get; set; }

    public bool IsForWholeSite { get; set; }

    public string? SpecificHost { get; set; }
}
