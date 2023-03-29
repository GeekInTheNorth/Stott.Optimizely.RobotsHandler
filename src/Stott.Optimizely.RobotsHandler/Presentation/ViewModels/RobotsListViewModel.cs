namespace Stott.Optimizely.RobotsHandler.Presentation.ViewModels;

using System.Collections.Generic;

public class RobotsListViewModel
{
    public string ApplicationName { get; internal set; }

    public string ApplicationVersion { get; internal set; }

    public List<RobotsListItemViewModel> List { get; set; }
}