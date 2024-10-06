using System;
using System.Collections.Generic;

namespace Stott.Optimizely.RobotsHandler.Environments;

public sealed class EnvironmentRobotsModel
{
    public Guid Id { get; set; }

    public string EnvironmentName { get; set; }

    public bool UseNoFollow { get; set; }

    public bool UseNoIndex { get; set; }

    public bool UseNoImageIndex { get; set; }

    public bool UseNoArchive { get; set; }

    public bool IsCurrentEnvironment { get; set; }

    public bool IsEnabled => UseNoFollow || UseNoIndex || UseNoImageIndex || UseNoArchive;

    public string ToMetaContent()
    {
        var options = new List<string>(0);

        if (UseNoIndex) { options.Add("noindex"); }
        if (UseNoFollow) { options.Add("nofollow"); }
        if (UseNoImageIndex) { options.Add("noimageindex"); }
        if (UseNoArchive) { options.Add("noarchive"); }

        return string.Join(",", options);
    }
}
