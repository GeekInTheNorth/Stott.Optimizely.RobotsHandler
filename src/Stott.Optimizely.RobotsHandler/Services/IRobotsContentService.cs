using System;
using System.Collections.Generic;

using Stott.Optimizely.RobotsHandler.Presentation.ViewModels;

namespace Stott.Optimizely.RobotsHandler.Services;

public interface IRobotsContentService
{
    IList<SiteRobotsViewModel> GetAll();

    SiteRobotsViewModel Get(Guid id);

    SiteRobotsViewModel GetDefault(Guid siteId);

    string GetRobotsContent(Guid siteId);

    string GetDefaultRobotsContent();

    void Save(SaveRobotsModel model);

    void Delete(Guid id);

    bool DoesConflictExists(SaveRobotsModel model);
}