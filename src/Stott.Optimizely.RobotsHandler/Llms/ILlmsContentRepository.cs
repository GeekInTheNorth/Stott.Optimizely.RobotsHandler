using System;
using System.Collections.Generic;

using Stott.Optimizely.RobotsHandler.Models;

namespace Stott.Optimizely.RobotsHandler.Llms;

public interface ILlmsContentRepository
{
    List<LlmsTxtEntity> GetAll();

    List<LlmsTxtEntity> GetAllForSite(Guid siteId);

    LlmsTxtEntity Get(Guid id);

    void Save(SaveLlmsModel model);

    void Delete(Guid id);
}