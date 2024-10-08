using EPiServer.Data;
using EPiServer.Data.Dynamic;

namespace Stott.Optimizely.RobotsHandler.Models;

[EPiServerDataStore(AutomaticallyCreateStore = true, AutomaticallyRemapStore = true)]
public class EnvironmentRobotsEntity : IDynamicData
{
    public Identity Id { get; set; }

    public string EnvironmentName { get; set; }

    public bool UseNoFollow { get; set; }

    public bool UseNoIndex { get; set; }

    public bool UseNoImageIndex { get; set; }

    public bool UseNoArchive { get; set; }
}
