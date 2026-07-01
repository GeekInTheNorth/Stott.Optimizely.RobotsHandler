using System;

namespace Stott.Optimizely.RobotsHandler.QueryRules;

public interface IQueryStringRule
{
    Guid GetId();

    string? QueryName { get; set; }

    string? MatchRule { get; set; }

    bool IsEnabled { get; set; }

    string? RobotsValue { get; set; }
}