using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EPiServer.Web;

using Stott.Optimizely.RobotsHandler.Models;
using Stott.Optimizely.RobotsHandler.Presentation.ViewModels;

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

    public IList<SiteRobotsViewModel> GetAll()
    {
        var robotRecords = robotsContentRepository.GetAll();
        var sites = siteDefinitionRepository.List();
        var models = new List<SiteRobotsViewModel>();

        foreach (var robotRecord in robotRecords)
        {
            var site = sites.FirstOrDefault(x => x.Id.Equals(robotRecord.SiteId));
            if (site != null)
            {
                models.Add(ToModel(robotRecord, site));
            }
        }

        foreach (var site in sites)
        {
            if (!models.Any(x => x.SiteId == site.Id && x.IsForWholeSite))
            {
                models.Add(ToModel(site));
            }
        }

        return models.OrderBy(x => x.SiteName).ThenBy(x => x.SpecificHost).ToList();
    }

    public SiteRobotsViewModel Get(Guid id)
    {
        var robotRecord = robotsContentRepository.Get(id);
        var sites = siteDefinitionRepository.List();
        var site = sites.FirstOrDefault(x => x.Id.Equals(robotRecord.SiteId));

        return ToModel(robotRecord, site);
    }

    public SiteRobotsViewModel GetDefault(Guid siteId)
    {
        var site = siteDefinitionRepository.Get(siteId);
        if (site == null)
        {
            throw new ArgumentException($"{nameof(siteId)} does not correlate to a known site.", nameof(siteId));
        }

        return ToModel(site);
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

    public void SaveRobotsContent(SaveRobotsModel model)
    {
        if (Guid.Empty.Equals(model.SiteId))
        {
            throw new ArgumentException($"{nameof(model)}.{nameof(model.SiteId)} must not be null or empty.", nameof(model));
        }

        var existingSite = siteDefinitionRepository.Get(model.SiteId);
        if (existingSite == null)
        {
            throw new ArgumentException($"{nameof(model)}.{nameof(model.SiteId)} does not correlate to a known site.", nameof(model));
        }

        robotsContentRepository.Save(model);
    }

    private static IEnumerable<KeyValuePair<string, string>> GetHosts(SiteDefinition siteDefinition)
    {
        yield return new KeyValuePair<string, string>("Default", string.Empty);

        if (siteDefinition is not { Hosts.Count: > 0 })
        {
            yield break;
        }

        foreach (var host in siteDefinition.Hosts)
        {
            yield return new KeyValuePair<string, string>(host.Name, host.Url.ToString());
        }
    }

    private static SiteRobotsViewModel ToModel(RobotsEntity robotsEntity, SiteDefinition siteDefinition)
    {
        return new SiteRobotsViewModel
        {
            Id = robotsEntity.Id.ExternalId,
            SiteId = robotsEntity.SiteId,
            IsForWholeSite = robotsEntity.IsForWholeSite || string.IsNullOrWhiteSpace(robotsEntity.SpecificHost),
            SpecificHost = robotsEntity.SpecificHost,
            RobotsContent = robotsEntity.RobotsContent,
            SiteName = siteDefinition.Name,
            AvailableHosts = GetHosts(siteDefinition).ToList()
        };
    }

    private SiteRobotsViewModel ToModel(SiteDefinition siteDefinition)
    {
        return new SiteRobotsViewModel
        {
            Id = Guid.Empty,
            SiteId = siteDefinition.Id,
            IsForWholeSite = true,
            RobotsContent = GetDefaultRobotsContent(),
            SiteName = siteDefinition.Name,
            AvailableHosts = GetHosts(siteDefinition).ToList()
        };
    }
}
