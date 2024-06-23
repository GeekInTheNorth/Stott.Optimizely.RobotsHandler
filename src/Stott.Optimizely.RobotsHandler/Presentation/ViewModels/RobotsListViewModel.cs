namespace Stott.Optimizely.RobotsHandler.Presentation.ViewModels;

using System.Collections.Generic;

using Stott.Optimizely.RobotsHandler.Services;

public class RobotsListViewModel
{
    public IList<SiteRobotsViewModel> List { get; set; }
}