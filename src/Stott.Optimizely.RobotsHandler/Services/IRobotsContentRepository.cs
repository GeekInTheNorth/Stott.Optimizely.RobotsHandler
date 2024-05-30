using System;
using System.Collections.Generic;

using Stott.Optimizely.RobotsHandler.Models;

namespace Stott.Optimizely.RobotsHandler.Services;

public interface IRobotsContentRepository
{
    List<RobotsEntity> GetAll();

    RobotsEntity Get(Guid id);

    void Save(Guid siteId, string robotsContent);
}
