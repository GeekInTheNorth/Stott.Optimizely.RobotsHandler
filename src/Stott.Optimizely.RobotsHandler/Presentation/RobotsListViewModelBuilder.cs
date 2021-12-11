using System.Linq;

using EPiServer.Web;

using Stott.Optimizely.RobotsHandler.Presentation.ViewModels;

namespace Stott.Optimizely.RobotsHandler.Presentation
{
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
    }
}
