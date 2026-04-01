using System.Collections.Generic;
using System.Linq;

using EPiServer.Applications;

using Stott.Optimizely.RobotsHandler.Extensions;

namespace Stott.Optimizely.RobotsHandler.Applications;

internal static class ApplicationMapper
{
    internal static List<HostViewModel> CreateHostSummaries(string defaultHostName)
    {
        return
        [
            new HostViewModel
            {
                DisplayName = defaultHostName,
                HostName = string.Empty
            }
        ];
    }

    internal static IEnumerable<HostViewModel> CreateHostSummaries(IList<ApplicationHost>? hostDefinitions)
    {
        if (hostDefinitions is not { Count: > 0 })
        {
            yield break;
        }

        yield return new HostViewModel { DisplayName = "Default", HostName = string.Empty };

        if (hostDefinitions is not { Count: > 0 })
        {
            yield break;
        }

        foreach (var host in hostDefinitions.Where(x => x.Url is not null))
        {
            yield return new HostViewModel { DisplayName = host.Url?.ToString(), HostName = host.Url.GetSanitizedHostDomain() };
        }
    }
}