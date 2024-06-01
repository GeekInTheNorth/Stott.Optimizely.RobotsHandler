using System.Collections.Generic;
using System.Linq;

using EPiServer.Web;

namespace Stott.Optimizely.RobotsHandler.Extensions;

public static class SiteDefinitionExtensions
{
    public static IEnumerable<KeyValuePair<string, string>> ToHostSummaries(this IList<HostDefinition> hostDefinitions)
    {
        yield return new KeyValuePair<string, string>("Default", string.Empty);
        if (hostDefinitions is not { Count: > 0 })
        {
            yield break;
        }

        foreach (var host in hostDefinitions.Where(x => x.Url is not null))
        {
            yield return new KeyValuePair<string, string>(host.Name, host.Url.ToString());
        }
    }
}
