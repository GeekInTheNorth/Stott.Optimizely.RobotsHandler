using EPiServer.Data;
using EPiServer.Data.Dynamic;

namespace Stott.Optimizely.RobotsHandler.Models;

[EPiServerDataStore(AutomaticallyCreateStore = true, AutomaticallyRemapStore = true)]
public class OpalTokenEntity : IDynamicData
{
    public Identity Id { get; set; }

    public string Name { get; set; }

    public string Scope { get; set; }

    public string Token { get; set; }
}