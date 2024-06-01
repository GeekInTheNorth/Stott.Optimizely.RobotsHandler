using System;
using System.Collections.Generic;
using System.Linq;

using EPiServer.Data;
using EPiServer.Data.Dynamic;

using Stott.Optimizely.RobotsHandler.Models;
using Stott.Optimizely.RobotsHandler.Presentation.ViewModels;

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
}
