using System;

using EPiServer.Data;
using EPiServer.Data.Dynamic;

namespace Stott.Optimizely.RobotsHandler.Models;

[EPiServerDataStore(AutomaticallyCreateStore = true, AutomaticallyRemapStore = true)]
public class LlmsTxtEntity : IDynamicData
{
    public Identity Id { get; set; } = Identity.NewIdentity(Guid.NewGuid());

    public string? AppId { get; set; }

    public bool IsForWholeSite { get; set; }

    public string? SpecificHost { get; set; }

    public string? LlmsContent { get; set; }
}