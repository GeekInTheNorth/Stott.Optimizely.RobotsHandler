using System;
using System.Collections.Generic;

using Stott.Optimizely.RobotsHandler.Models;

namespace Stott.Optimizely.RobotsHandler.Robots;

public interface IRobotsContentRepository
{
    List<RobotsEntity> GetAll();

    List<RobotsEntity> GetAllForSite(Guid siteId);

    RobotsEntity Get(Guid id);

    void Save(SaveRobotsModel model);

    void Delete(Guid id);
}
