using System.Collections.Generic;

namespace Stott.Optimizely.RobotsHandler.Llms;

public sealed class LlmsListViewModel
{
    public IList<SiteLlmsViewModel> List { get; set; }
}
