using System;

namespace Stott.Optimizely.RobotsHandler.QueryRules;

public interface IQueryStringRule
{
    Guid GetId();

    string? QueryName { get; }

    string? MatchRule { get; }

    bool IsEnabled { get; }

    string? RobotsValue { get; }
}