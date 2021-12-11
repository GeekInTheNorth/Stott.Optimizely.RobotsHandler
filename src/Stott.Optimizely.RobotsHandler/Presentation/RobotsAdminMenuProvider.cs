using System.Collections.Generic;

using EPiServer.Shell.Navigation;

namespace Stott.Optimizely.RobotsHandler.Presentation
{
    [MenuProvider]
    public class RobotsAdminMenuProvider : IMenuProvider
    {
        public IEnumerable<MenuItem> GetMenuItems()
        {
            var listMenuItem = new UrlMenuItem("Robots", "/global/cms/admin/stott.optimizely.robots", "/Robots/List")
            {
                IsAvailable = context => true,
                SortIndex = 900
            };

            return new List<MenuItem> { listMenuItem };
        }
    }
}
