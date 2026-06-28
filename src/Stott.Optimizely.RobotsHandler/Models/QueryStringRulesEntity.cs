using System;

using EPiServer.Data;
using EPiServer.Data.Dynamic;

using Stott.Optimizely.RobotsHandler.QueryRules;

namespace Stott.Optimizely.RobotsHandler.Models;

[EPiServerDataStore(AutomaticallyCreateStore = true, AutomaticallyRemapStore = true)]
public class QueryStringRulesEntity : IDynamicData, IQueryStringRule
{
    public Identity Id { get; set; } = Identity.NewIdentity(Guid.NewGuid());

    public string? QueryName { get; set; }

    public string? MatchRule { get; set; }

    public bool IsEnabled { get; set; }

    public string? RobotsValue { get; set; }

    public bool IsEnabledForATags { get; set; }

    public Guid GetId()
    {
        return Id?.ExternalId ?? Guid.Empty;
    }
}
