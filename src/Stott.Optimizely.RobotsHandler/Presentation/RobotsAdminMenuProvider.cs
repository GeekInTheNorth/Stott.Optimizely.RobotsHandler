using System.Collections.Generic;

using EPiServer.Shell.Navigation;

using Stott.Optimizely.RobotsHandler.Common;

namespace Stott.Optimizely.RobotsHandler.Presentation;

[MenuProvider]
public class RobotsAdminMenuProvider : IMenuProvider
{
    public IEnumerable<MenuItem> GetMenuItems()
    {
        yield return CreateMenuItem("Robots", "/global/cms/stott.optimizely.robotshandler", "/stott.robotshandler/administration/", SortIndex.Last + 1);
        yield return CreateMenuItem("Robots.txt Files", "/global/cms/stott.optimizely.robotshandler/sites", "/stott.robotshandler/administration/#robots-files", SortIndex.Last + 2);
        yield return CreateMenuItem("Environment Robots", "/global/cms/stott.optimizely.robotshandler/environments", "/stott.robotshandler/administration/#environment-robots", SortIndex.Last + 3);
        yield return CreateMenuItem("LLMS.txt Files", "/global/cms/stott.optimizely.robotshandler/llms", "/stott.robotshandler/administration/#llms-files", SortIndex.Last + 4);
        yield return CreateMenuItem("Opal Tools", "/global/cms/stott.optimizely.robotshandler/opaltools", "/stott.robotshandler/administration/#opal-tools", SortIndex.Last + 5);
    }

    private static UrlMenuItem CreateMenuItem(string name, string path, string url, int index)
    {
        return new UrlMenuItem(name, path, url)
        {
            IsAvailable = context => true,
            SortIndex = index,
            AuthorizationPolicy = RobotsConstants.AuthorizationPolicy
        };
    }
}
