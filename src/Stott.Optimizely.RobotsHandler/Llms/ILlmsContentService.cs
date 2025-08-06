using System;
using System.Collections.Generic;

namespace Stott.Optimizely.RobotsHandler.Llms;

public interface ILlmsContentService
{
    IList<SiteLlmsViewModel> GetAll();

    SiteLlmsViewModel Get(Guid id);

    SiteLlmsViewModel GetDefault(Guid siteId);

    string GetLlmsContent(Guid siteId, string host);

    string GetDefaultLlmsContent();

    void Save(SaveLlmsModel model);

    void Delete(Guid id);

    bool DoesConflictExists(SaveLlmsModel model);
}
