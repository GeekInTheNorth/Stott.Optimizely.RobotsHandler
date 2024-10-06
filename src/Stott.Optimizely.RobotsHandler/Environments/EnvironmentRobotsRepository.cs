using System;
using System.Collections.Generic;
using System.Linq;

using EPiServer.Data;
using EPiServer.Data.Dynamic;
using Stott.Optimizely.RobotsHandler.Models;

namespace Stott.Optimizely.RobotsHandler.Environments;

public sealed class EnvironmentRobotsRepository : IEnvironmentRobotsRepository
{
    private readonly DynamicDataStore store;

    public EnvironmentRobotsRepository()
    {
        store = DynamicDataStoreFactory.Instance.CreateStore(typeof(EnvironmentRobotsEntity));
    }

    public EnvironmentRobotsModel Get(string environmentName)
    {
        return store.Find<EnvironmentRobotsEntity>(new Dictionary<string, object> { { nameof(EnvironmentRobotsEntity.EnvironmentName), environmentName } })
                    .Select(x => ToModel(x))
                    .FirstOrDefault();
    }

    public IList<EnvironmentRobotsModel> GetAll()
    {
        return store.Find<EnvironmentRobotsEntity>(new Dictionary<string, object>())
                    .Select(x => ToModel(x))
                    .ToList();
    }

    public void Save(EnvironmentRobotsModel model)
    {
        var recordToSave = store.Find<EnvironmentRobotsEntity>(new Dictionary<string, object> { { nameof(EnvironmentRobotsEntity.EnvironmentName), model.EnvironmentName } }).FirstOrDefault();
        recordToSave ??= new EnvironmentRobotsEntity
        {
            Id = Identity.NewIdentity(Guid.NewGuid()),
            EnvironmentName = model.EnvironmentName,
        };

        recordToSave.UseNoIndex = model.UseNoIndex;
        recordToSave.UseNoFollow = model.UseNoFollow;
        recordToSave.UseNoImageIndex = model.UseNoImageIndex;
        recordToSave.UseNoArchive = model.UseNoArchive;

        store.Save(recordToSave);
    }

    private static EnvironmentRobotsModel ToModel(EnvironmentRobotsEntity entity)
    {
        return new EnvironmentRobotsModel
        {
            Id = entity.Id.ExternalId,
            EnvironmentName = entity.EnvironmentName,
            UseNoFollow = entity.UseNoFollow,
            UseNoIndex = entity.UseNoIndex,
            UseNoImageIndex = entity.UseNoImageIndex,
            UseNoArchive = entity.UseNoArchive
        };
    }
}
