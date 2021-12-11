using System;

namespace Stott.Optimizely.RobotsHandler.Services
{
    public interface IRobotsContentService
    {
        string GetRobotsContent(Guid siteId);

        string GetRobotsContent(string requestPath);

        string GetDefaultRobotsContent();

        void SaveRobotsContent(Guid siteId, string robotsContent);
    }
}
