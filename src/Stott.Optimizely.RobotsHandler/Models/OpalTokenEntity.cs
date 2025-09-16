using System;
using EPiServer.Data;
using EPiServer.Data.Dynamic;

namespace Stott.Optimizely.RobotsHandler.Models;

[EPiServerDataStore(AutomaticallyCreateStore = true, AutomaticallyRemapStore = true)]
public class OpalTokenEntity : IDynamicData
{
    public Identity Id { get; set; }

    public string Name { get; set; }

    public string RobotsScope { get; set; }

    public string LlmsScope { get; set; }

    public string TokenHash { get; set; }

    public string DisplayToken { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}