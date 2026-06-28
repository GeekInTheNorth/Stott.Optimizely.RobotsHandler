using System;

namespace Stott.Optimizely.RobotsHandler.QueryRules;

public sealed class SaveQueryRuleMode : IQueryStringRule
{
    public Guid Id { get; set; }
        
    public string? QueryName { get; set; }
        
    public string? MatchRule { get; set; }
        
    public bool IsEnabled { get; set; }
        
    public string? RobotsValue { get; set; }
        
    public bool IsEnabledForATags { get; set; }

    public Guid GetId() => Id;
}