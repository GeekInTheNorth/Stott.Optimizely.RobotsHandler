using System;
using System.Collections.Generic;

using Stott.Optimizely.RobotsHandler.Models;
using Stott.Optimizely.RobotsHandler.Presentation.ViewModels;

namespace Stott.Optimizely.RobotsHandler.Services;

public interface IRobotsContentRepository
{
    List<RobotsEntity> GetAll();

    List<RobotsEntity> GetAllForSite(Guid siteId);

    RobotsEntity Get(Guid id);

    void Save(SaveRobotsModel model);

    void Delete(Guid id);
}
