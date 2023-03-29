namespace Stott.Optimizely.RobotsHandler.Presentation;

using System.Linq;
using System.Reflection;

using EPiServer.Web;

using Stott.Optimizely.RobotsHandler.Presentation.ViewModels;

public class RobotsListViewModelBuilder : IRobotsListViewModelBuilder
{
    private readonly ISiteDefinitionRepository _siteDefinitionRepository;

    public RobotsListViewModelBuilder(ISiteDefinitionRepository siteDefinitionRepository)
    {
        _siteDefinitionRepository = siteDefinitionRepository;
    }

    public RobotsListViewModel Build()
    {
        return new RobotsListViewModel
        {
            ApplicationName = "Stott Robots Handler",
            ApplicationVersion = GetApplicationVersion(),
            List = _siteDefinitionRepository.List().Select(ToViewModel).ToList()
        };
    }

    private RobotsListItemViewModel ToViewModel(SiteDefinition siteDefinition)
    {
        return new RobotsListItemViewModel
        {
            Id = siteDefinition.Id,
            Name = siteDefinition.Name,
            Url = siteDefinition.SiteUrl.ToString()
        };
    }

    private static string GetApplicationVersion()
    {
        var assembly = Assembly.GetAssembly(typeof(RobotsEditViewModelBuilder));
        var assemblyName = assembly?.GetName();

        return $"v{assemblyName?.Version}";
    }
}