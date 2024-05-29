namespace Stott.Optimizely.RobotsHandler.Presentation;

using System.Linq;

using EPiServer.Web;

using Stott.Optimizely.RobotsHandler.Presentation.ViewModels;
using Stott.Optimizely.RobotsHandler.Services;

public class RobotsListViewModelBuilder : IRobotsListViewModelBuilder
{
    private readonly IRobotsContentService _robotsContentService;

    public RobotsListViewModelBuilder(IRobotsContentService robotsContentService)
    {
        _robotsContentService = robotsContentService;
    }

    public RobotsListViewModel Build()
    {
        return new RobotsListViewModel
        {
            List = _robotsContentService.GetRobots()
        };
    }
}