using System;

using Stott.Optimizely.RobotsHandler.Models;

namespace Stott.Optimizely.RobotsHandler.Services
{
    public interface IRobotsContentRepository
    {
        RobotsEntity Get(Guid siteId);

        void Save(Guid siteId, string robotsContent);
    }
}
