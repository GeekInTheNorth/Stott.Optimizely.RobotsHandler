using System;

namespace Stott.Optimizely.RobotsHandler.Services;

public class EnvironmentRobotsModel
{
    public Guid Id { get; set; }

    public string EnvironmentName { get; set; }

    public bool UseNoFollow { get; set; }

    public bool UseNoIndex { get; set; }

    public bool UseNoImageIndex { get; set; }

    public bool UseNoArchive { get; set; }

    public bool IsCurrentEnvironment { get; set; }
}
