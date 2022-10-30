using System.Collections.Generic;

using EPiServer.Authorization;
using EPiServer.Shell.Navigation;

using Stott.Optimizely.RobotsHandler.Common;

namespace Stott.Optimizely.RobotsHandler.Presentation
{
    [MenuProvider]
    public class RobotsAdminMenuProvider : IMenuProvider
    {
        public IEnumerable<MenuItem> GetMenuItems()
        {
            var listMenuItem = new UrlMenuItem("Robots", "/global/cms/stott.optimizely.robots", "/Robots/List")
            {
                IsAvailable = context => true,
                SortIndex = SortIndex.Last + 1,
                AuthorizationPolicy = RobotsConstants.AuthorizationPolicy
            };

            return new List<MenuItem> { listMenuItem };
        }
    }
}
