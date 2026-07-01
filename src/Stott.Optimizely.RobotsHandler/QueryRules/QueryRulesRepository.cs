using System;
using System.Collections.Generic;
using System.Linq;

using EPiServer.Data;
using EPiServer.Data.Dynamic;

using Stott.Optimizely.RobotsHandler.Models;

namespace Stott.Optimizely.RobotsHandler.QueryRules;

public sealed class QueryRulesRepository : IQueryRulesRepository
{
    private readonly DynamicDataStore store;

    public QueryRulesRepository()
    {
        store = DynamicDataStoreFactory.Instance.CreateStore(typeof(QueryStringRulesEntity));
    }

    public void Delete(Guid id)
    {
        store.Delete(Identity.NewIdentity(id));
    }

    public IQueryStringRule? Get(Guid id)
    {
        return GetEntity(id);
    }

    public List<IQueryStringRule> GetAll()
    {
        var rules = store.Find<QueryStringRulesEntity>(new Dictionary<string, object>()).ToList();

        return rules.Cast<IQueryStringRule>().ToList();
    }

    public void Save(IQueryStringRule model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var recordToSave = GetEntity(model.GetId());
        recordToSave ??= new QueryStringRulesEntity { Id = Identity.NewIdentity(Guid.NewGuid()) };

        recordToSave.QueryName = model.QueryName;
        recordToSave.RobotsValue = model.RobotsValue;
        recordToSave.MatchRule = model.MatchRule;
        recordToSave.IsEnabled = model.IsEnabled;

        store.Save(recordToSave);
    }

    private QueryStringRulesEntity? GetEntity(Guid id)
    {
        if (Guid.Empty.Equals(id))
        {
            return null;
        }

        return store.Load<QueryStringRulesEntity>(Identity.NewIdentity(id));
    }
}