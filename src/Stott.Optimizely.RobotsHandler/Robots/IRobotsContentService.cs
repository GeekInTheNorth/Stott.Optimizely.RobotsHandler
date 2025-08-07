using System;
using System.Collections.Generic;

namespace Stott.Optimizely.RobotsHandler.Robots;

public interface IRobotsContentService
{
    IList<SiteRobotsViewModel> GetAll();

    SiteRobotsViewModel Get(Guid id);

    SiteRobotsViewModel GetDefault(Guid siteId);

    string GetRobotsContent(Guid siteId, string host);

    string GetDefaultRobotsContent();

    void Save(SaveRobotsModel model);

    void Delete(Guid id);

    bool DoesConflictExists(SaveRobotsModel model);
}