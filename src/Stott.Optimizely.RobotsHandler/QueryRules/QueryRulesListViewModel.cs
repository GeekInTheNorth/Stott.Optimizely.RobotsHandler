using System.Collections.Generic;

namespace Stott.Optimizely.RobotsHandler.QueryRules;

public sealed class QueryRulesListViewModel(List<IQueryStringRule> data)
{
    public IList<IQueryStringRule> List { get; set; } = data ?? [];
}