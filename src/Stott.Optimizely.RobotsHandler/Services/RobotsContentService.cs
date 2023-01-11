using System;
using System.Text;

using EPiServer.Web;

namespace Stott.Optimizely.RobotsHandler.Services
{
    public class RobotsContentService : IRobotsContentService
    {
        private readonly ISiteDefinitionRepository siteDefinitionRepository;

        private readonly IRobotsContentRepository robotsContentRepository;

        public RobotsContentService(
            ISiteDefinitionRepository siteDefinitionRepository,
            IRobotsContentRepository robotsContentRepository)
        {
            this.siteDefinitionRepository = siteDefinitionRepository;
            this.robotsContentRepository = robotsContentRepository;
        }

        public string GetDefaultRobotsContent()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("User-agent: *");
            stringBuilder.AppendLine("Disallow: /episerver/");
            stringBuilder.AppendLine("Disallow: /utils/");

            return stringBuilder.ToString();
        }

        public string GetRobotsContent(Guid siteId)
        {
            var robots = robotsContentRepository.Get(siteId);
            if (robots == null)
            {
                return GetDefaultRobotsContent();
            }

            return robots.RobotsContent;
        }

        public void SaveRobotsContent(Guid siteId, string robotsContent)
        {
            if (Guid.Empty.Equals(siteId))
            {
                throw new ArgumentException($"{nameof(siteId)} is not a non-null non-empty value.", nameof(siteId));
            }

            var existingSite = siteDefinitionRepository.Get(siteId);
            if (existingSite == null)
            {
                throw new ArgumentException($"{nameof(siteId)} does not correlate to a known site.", nameof(siteId));
            }

            robotsContentRepository.Save(existingSite.Id, robotsContent);
        }
    }
}
