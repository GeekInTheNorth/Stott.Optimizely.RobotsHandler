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
    private readonly ITokenHashService _hashService;

    public OpalTokenRepository(DynamicDataStoreFactory dataStoreFactory, ITokenHashService hashService)
    {
        store = dataStoreFactory.CreateStore(typeof(OpalTokenEntity));
        _hashService = hashService;
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
            Id = Identity.NewIdentity(Guid.NewGuid()),
            TokenSalt = Guid.NewGuid().ToString().Replace("-", string.Empty)
        };

        recordToSave.Name = saveModel.Name;
        recordToSave.RobotsScope = saveModel.RobotsScope;
        recordToSave.LlmsScope = saveModel.LlmsScope;

        if (!string.IsNullOrWhiteSpace(saveModel.Token))
        {
            recordToSave.TokenHash = _hashService.HashToken(saveModel.Token, recordToSave.TokenSalt);
            recordToSave.DisplayToken = $"{saveModel.Token[..4]}...";
        }

        store.Save(recordToSave);
    }

    public TokenModel GetByToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        var allTokens = store.Find<OpalTokenEntity>(new Dictionary<string, object>()).ToList();
        var matchingHashedToken = allTokens.FirstOrDefault(t => !string.IsNullOrWhiteSpace(t.TokenHash) && _hashService.VerifyToken(token, t.TokenHash, t.TokenSalt));

        return ToModel(matchingHashedToken);
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
            Token = entity.DisplayToken
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