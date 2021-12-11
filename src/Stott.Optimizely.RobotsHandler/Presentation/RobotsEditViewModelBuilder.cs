using System;

using EPiServer.Web;

using Stott.Optimizely.RobotsHandler.Exceptions;
using Stott.Optimizely.RobotsHandler.Presentation.ViewModels;
using Stott.Optimizely.RobotsHandler.Services;

namespace Stott.Optimizely.RobotsHandler.Presentation
{
    public class RobotsEditViewModelBuilder : IRobotsEditViewModelBuilder
    {
        private readonly ISiteDefinitionRepository _siteDefinitionRepository;

        private readonly IRobotsContentService _robotsContentService;

        private Guid _siteId;

        public RobotsEditViewModelBuilder(
            ISiteDefinitionRepository siteDefinitionRepository,
            IRobotsContentService robotsContentService)
        {
            _siteDefinitionRepository = siteDefinitionRepository;
            _robotsContentService = robotsContentService;
            _siteId = Guid.Empty;
        }

        public RobotsEditViewModel Build()
        {
            if (_siteId == Guid.Empty)
            {
                throw new RobotsInvalidSiteIdException(_siteId);
            }

            var selectedSite = _siteDefinitionRepository.Get(_siteId);
            if (selectedSite == null)
            {
                throw new RobotsInvalidSiteException($"'{_siteId}' does not refer to a valid site.");
            }

            var robotsContent = _robotsContentService.GetRobotsContent(_siteId);

            return new RobotsEditViewModel
            {
                SiteId = selectedSite.Id,
                SiteName = selectedSite.Name,
                RobotsContent = robotsContent
            };
        }

        public IRobotsEditViewModelBuilder WithSiteId(Guid siteId)
        {
            _siteId = siteId;

            return this;
        }
    }
}
