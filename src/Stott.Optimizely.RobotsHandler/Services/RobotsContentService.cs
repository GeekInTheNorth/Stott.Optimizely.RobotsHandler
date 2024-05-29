using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EPiServer.Web;

namespace Stott.Optimizely.RobotsHandler.Services;

public sealed class RobotsContentService : IRobotsContentService
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

    public IList<SiteRobotsViewModel> GetRobots()
    {
        var robotRecords = robotsContentRepository.GetAll();
        var sites = siteDefinitionRepository.List();
        var models = new List<SiteRobotsViewModel>();

        foreach (var model in robotRecords)
        {
            var site = sites.FirstOrDefault(x => x.Id.Equals(model.SiteId));
            if (site != null)
            {
                models.Add(new SiteRobotsViewModel
                {
                    Id = model.Id.ExternalId,
                    SiteId = model.SiteId,
                    IsForWholeSite = model.IsForWholeSite || string.IsNullOrWhiteSpace(model.SpecificHost),
                    SpecificHost = model.SpecificHost,
                    RobotsContent = model.RobotsContent,
                    SiteName = site.Name,
                    AvailableHosts = GetHosts(site).ToList()
                });
            }
        }

        return models;
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

    private static IEnumerable<KeyValuePair<string, string>> GetHosts(SiteDefinition siteDefinition)
    {
        if (siteDefinition is not { Hosts.Count: > 0 })
        {
            yield break;
        }

        foreach (var host in siteDefinition.Hosts)
        {
            yield return new KeyValuePair<string, string>(host.Name, host.Url.ToString());
        }
    }
}
