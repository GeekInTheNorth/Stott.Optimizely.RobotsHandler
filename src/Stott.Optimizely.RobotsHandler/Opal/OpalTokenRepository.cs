using System;
using System.Collections.Generic;
using System.Linq;

using EPiServer.Data;
using EPiServer.Data.Dynamic;

using Stott.Optimizely.RobotsHandler.Models;

namespace Stott.Optimizely.RobotsHandler.Opal;

public class OpalTokenRepository : IOpalTokenRepository
{
    private readonly DynamicDataStore store;

    public OpalTokenRepository(DynamicDataStoreFactory dataStoreFactory)
    {
        store = dataStoreFactory.CreateStore(typeof(OpalTokenEntity));
    }

    public void Delete(Guid id)
    {
        store.Delete(Identity.NewIdentity(id));
    }

    public List<TokenModel> List()
    {
        var records = store.Find<OpalTokenEntity>(new Dictionary<string, object>()).ToList();

        return records.Select(ToModel).ToList();
    }

    public void Save(TokenModel saveModel)
    {
        var recordToSave = Get(saveModel.Id);
        recordToSave ??= new OpalTokenEntity
        {
            Id = Identity.NewIdentity(Guid.NewGuid())
        };

        recordToSave.Name = saveModel.Name;
        recordToSave.Token = saveModel.Token;
        recordToSave.RobotsScope = saveModel.RobotsScope;
        recordToSave.LlmsScope = saveModel.LlmsScope;

        store.Save(recordToSave);
    }

    private static TokenModel ToModel(OpalTokenEntity entity)
    {
        if (entity is null)
        {
            return null;
        }

        return new TokenModel
        {
            Id = entity.Id.ExternalId,
            Name = entity.Name,
            RobotsScope = entity.RobotsScope,
            LlmsScope = entity.LlmsScope,
            Token = entity.Token
        };
    }

    private OpalTokenEntity Get(Guid id)
    {
        if (Guid.Empty.Equals(id))
        {
            return null;
        }

        return store.Load<OpalTokenEntity>(Identity.NewIdentity(id));
    }
}