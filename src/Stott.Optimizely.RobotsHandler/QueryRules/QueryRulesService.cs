using System;
using System.Collections.Generic;
using System.Linq;

using Stott.Optimizely.RobotsHandler.Cache;

namespace Stott.Optimizely.RobotsHandler.QueryRules;

public sealed class QueryRulesService(Lazy<IQueryRulesRepository> repository, IRobotsCacheHandler cacheHandler) : IQueryRulesService
{
    private const string GetAllCacheKey = "srh:qr:all";

    private const string GetCacheKey = "srh:qr:get";

    public List<IQueryStringRule> GetAll()
    {
        var data = cacheHandler.Get(GetAllCacheKey, () => repository.Value.GetAll());

        return data ?? [];
    }
    
    public IQueryStringRule? Get(Guid id)
    {
        var cacheKey = $"{GetCacheKey}:{id}";
        var data = cacheHandler.Get(cacheKey, () => repository.Value.Get(id));

        return data;
    }
    
    public void Save(IQueryStringRule model)
    {
        repository.Value.Save(model);

        cacheHandler.RemoveAll();
    }
    
    public void Delete(Guid id)
    {
        repository.Value.Delete(id);

        cacheHandler.RemoveAll();
    }

    public bool DoesConflictExists(IQueryStringRule model)
    {
        var allRules = GetAll();

        return allRules.Any(r => 
            string.Equals(r.QueryName, model.QueryName, StringComparison.OrdinalIgnoreCase) && 
            string.Equals(r.MatchRule, model.MatchRule, StringComparison.OrdinalIgnoreCase) &&
            !Guid.Equals(r.GetId(), model.GetId()));
    }
}