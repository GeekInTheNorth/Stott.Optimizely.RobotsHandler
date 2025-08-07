using System;
using System.Collections.Generic;
using System.Linq;

using EPiServer.Data;
using EPiServer.Data.Dynamic;

using Stott.Optimizely.RobotsHandler.Models;

namespace Stott.Optimizely.RobotsHandler.Robots;

public sealed class RobotsContentRepository : IRobotsContentRepository
{
    private readonly DynamicDataStore store;

    public RobotsContentRepository()
    {
        store = DynamicDataStoreFactory.Instance.CreateStore(typeof(RobotsEntity));
    }

    public RobotsEntity Get(Guid id)
    {
        if (Guid.Empty.Equals(id))
        {
            return null;
        }

        return store.Load<RobotsEntity>(Identity.NewIdentity(id));
    }

    public List<RobotsEntity> GetAll()
    {
        return store.Find<RobotsEntity>(new Dictionary<string, object>()).ToList();
    }

    public List<RobotsEntity> GetAllForSite(Guid siteId)
    {
        return store.Find<RobotsEntity>(new Dictionary<string, object> { { nameof(RobotsEntity.SiteId), siteId } }).ToList();
    }

    public void Save(SaveRobotsModel model)
    {
        var recordToSave = Get(model.Id);
        recordToSave ??= new RobotsEntity
        {
            Id = Identity.NewIdentity(Guid.NewGuid()),
            SiteId = model.SiteId,
        };

        recordToSave.SpecificHost = model.SpecificHost;
        recordToSave.IsForWholeSite = string.IsNullOrWhiteSpace(model.SpecificHost);
        recordToSave.RobotsContent = model.RobotsContent;

        store.Save(recordToSave);
    }

    public void Delete(Guid id)
    {
        store.Delete(Identity.NewIdentity(id));
    }
}
