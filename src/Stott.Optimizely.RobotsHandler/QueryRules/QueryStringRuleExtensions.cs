using System.Collections.Generic;

namespace Stott.Optimizely.RobotsHandler.QueryRules;

public static class QueryStringRuleExtensions
{
    public static IEnumerable<QueryStringRuleModel> ToModels(this IList<IQueryStringRule>? rules)
    {
        if (rules == null)
        {
            yield break;
        }

        foreach(var rule in rules)
        {
            var model = rule.ToModel();
            if (model is not null)
            {
                yield return model;
            }
        }
    }

    public static QueryStringRuleModel? ToModel(this IQueryStringRule? rule)
    {
        if (rule is null)
        {
            return null;
        }

        return new QueryStringRuleModel
        {
            Id = rule.GetId(),
            QueryName = rule.QueryName,
            MatchRule = rule.MatchRule,
            RobotsValue = rule.RobotsValue,
            IsEnabled = rule.IsEnabled
        };
    }
}
