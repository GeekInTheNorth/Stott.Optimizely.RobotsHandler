using System.Collections.Generic;

namespace Stott.Optimizely.RobotsHandler.Llms;

public sealed class LlmsListViewModel
{
    public IList<ApplicationLlmsViewModel> List { get; set; } = [];
}
