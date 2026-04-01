using System;
using System.Collections.Generic;

namespace Stott.Optimizely.RobotsHandler.Llms;

public interface ILlmsContentService
{
    IList<ApplicationLlmsViewModel> GetAll();

    ApplicationLlmsViewModel Get(Guid id);

    ApplicationLlmsViewModel GetDefault(string? appId);

    string? GetLlmsContent(string? appId, string? host);

    string? GetDefaultLlmsContent();

    void Save(SaveLlmsModel model);

    void Delete(Guid id);

    bool DoesConflictExists(SaveLlmsModel model);
}
