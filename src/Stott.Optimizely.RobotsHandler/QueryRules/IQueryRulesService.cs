using System;
using System.Collections.Generic;

namespace Stott.Optimizely.RobotsHandler.QueryRules;

public interface IQueryRulesService
{
    List<IQueryStringRule> GetAll();

    IQueryStringRule? Get(Guid id);

    void Save(IQueryStringRule model);

    void Delete(Guid id);

    bool DoesConflictExists(IQueryStringRule model);
}