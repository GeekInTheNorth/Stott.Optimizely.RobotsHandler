using System.Collections.Generic;

namespace Stott.Optimizely.RobotsHandler.Applications;

public sealed class ApplicationViewModel
{
    public string? AppId { get; set; }

    public string? AppName { get; set; }

    public List<HostViewModel>? AvailableHosts { get; set; }
}