using System;
using System.Collections.Generic;
using System.Linq;

using EPiServer.Data;
using EPiServer.Data.Dynamic;

using Stott.Optimizely.RobotsHandler.Models;

namespace Stott.Optimizely.RobotsHandler.Llms;

public sealed class DefaultLlmsContentRepository : ILlmsContentRepository
{
    private readonly DynamicDataStore store;

    public DefaultLlmsContentRepository()
    {
        store = DynamicDataStoreFactory.Instance.CreateStore(typeof(LlmsTxtEntity));
    }

    public void Delete(Guid id)
    {
        store.Delete(Identity.NewIdentity(id));
    }

    public LlmsTxtEntity Get(Guid id)
    {
        if (Guid.Empty.Equals(id))
        {
            return null;
        }

        return store.Load<LlmsTxtEntity>(Identity.NewIdentity(id));
    }

    public List<LlmsTxtEntity> GetAll()
    {
        return store.Find<LlmsTxtEntity>(new Dictionary<string, object>()).ToList();
    }

    public List<LlmsTxtEntity> GetAllForSite(Guid siteId)
    {
        return store.Find<LlmsTxtEntity>(new Dictionary<string, object> { { nameof(LlmsTxtEntity.SiteId), siteId } }).ToList();
    }

    public void Save(SaveLlmsModel model)
    {
        var recordToSave = Get(model.Id);
        recordToSave ??= new LlmsTxtEntity
        {
            Id = Identity.NewIdentity(Guid.NewGuid()),
            SiteId = model.SiteId,
        };

        recordToSave.SpecificHost = model.SpecificHost;
        recordToSave.IsForWholeSite = string.IsNullOrWhiteSpace(model.SpecificHost);
        recordToSave.LlmsContent = model.LlmsContent;

        store.Save(recordToSave);
    }
}