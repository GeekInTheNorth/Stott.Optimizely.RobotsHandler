using System.Collections.Generic;

namespace Stott.Optimizely.RobotsHandler.QueryRules;

public sealed class QueryRulesListViewModel(IList<IQueryStringRule> data)
{
    public IList<QueryStringRuleModel> List { get; } = [.. data.ToModels()];
}