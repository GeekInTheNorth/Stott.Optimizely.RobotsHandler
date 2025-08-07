﻿using System.Collections.Generic;
using System.Linq;

using EPiServer.Web;

using Stott.Optimizely.RobotsHandler.Sites;

namespace Stott.Optimizely.RobotsHandler.Extensions;

public static class SiteDefinitionExtensions
{
    public static IEnumerable<SiteHostViewModel> ToHostSummaries(this IList<HostDefinition> hostDefinitions)
    {
        yield return new SiteHostViewModel { DisplayName = "Default", HostName = string.Empty };
        if (hostDefinitions is not { Count: > 0 })
        {
            yield break;
        }

        foreach (var host in hostDefinitions.Where(x => x.Url is not null))
        {
            yield return new SiteHostViewModel { DisplayName = host.Name, HostName = host.Name };
        }
    }
}
