using System;
using System.Collections.Generic;
using System.Linq;

using EPiServer.Data;
using EPiServer.Data.Dynamic;

using Stott.Optimizely.RobotsHandler.Models;

namespace Stott.Optimizely.RobotsHandler.Services;

public sealed class RobotsContentRepository : IRobotsContentRepository
{
    private readonly DynamicDataStore store;

    public RobotsContentRepository()
    {
        store = DynamicDataStoreFactory.Instance.CreateStore(typeof(RobotsEntity));
    }

    public RobotsEntity Get(Guid id)
    {
        return store.Load<RobotsEntity>(Identity.NewIdentity(id));
    }

    public List<RobotsEntity> GetAll()
    {
        return store.Find<RobotsEntity>(new Dictionary<string, object>()).ToList();
    }

    public void Save(Guid siteId, string robotsContent)
    {
        var recordToSave = Get(siteId);
        recordToSave ??= new RobotsEntity
        {
            Id = Identity.NewIdentity(siteId),
            SiteId = siteId
        };

        recordToSave.RobotsContent = robotsContent;

        store.Save(recordToSave);
    }
}
