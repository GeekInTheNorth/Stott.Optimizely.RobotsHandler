using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EPiServer.Web;

using Stott.Optimizely.RobotsHandler.Extensions;
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
        stringBuilder.AppendLine("Disallow: /");

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
        if (robotRecord == null)
        {
            throw new RobotsEntityNotFoundException(id);
        }

        var sites = siteDefinitionRepository.List();
        var site = sites.FirstOrDefault(x => x.Id.Equals(robotRecord.SiteId));
        if (site == null)
        {
            throw new RobotsEntityNotFoundException($"Robotes entity with id '{id}' not match a site definition.");
        }

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

    public string GetRobotsContent(Guid siteId, string host)
    {
        var robots = robotsContentRepository.GetAllForSite(siteId) ?? new List<RobotsEntity>(0);
        var matchingRobots = robots.FirstOrDefault(x => string.Equals(x.SpecificHost, host, StringComparison.OrdinalIgnoreCase)) ??
                             robots.FirstOrDefault(x => string.IsNullOrWhiteSpace(x.SpecificHost));

        if (matchingRobots == null)
        {
            return GetDefaultRobotsContent();
        }

        return matchingRobots.RobotsContent;
    }

    public void Save(SaveRobotsModel model)
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

    public void Delete(Guid id)
    {
        if (Guid.Empty.Equals(id))
        {
            return;
        }
        
        robotsContentRepository.Delete(id);
    }

    public bool DoesConflictExists(SaveRobotsModel model)
    {
        var existingConfigurations = robotsContentRepository.GetAll() ?? new List<RobotsEntity>(0);
        return existingConfigurations.Any(x => IsConflict(model, x));
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
            AvailableHosts = siteDefinition.Hosts.ToHostSummaries().ToList(),
            CanDelete = true
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
            AvailableHosts = siteDefinition.Hosts.ToHostSummaries().ToList()
        };
    }

    private static bool IsConflict(SaveRobotsModel model, RobotsEntity entity)
    {
        if (Guid.Empty.Equals(model.Id))
        {
            return model.SiteId == entity.SiteId &&
                   string.IsNullOrWhiteSpace(model.SpecificHost) == string.IsNullOrWhiteSpace(entity.SpecificHost);
        }

        var modelHost = model.SpecificHost ?? string.Empty;
        var entityHost = entity.SpecificHost ?? string.Empty;

        return Guid.Equals(model.SiteId, entity.SiteId) && !Guid.Equals(model.Id, entity.Id.ExternalId) &&
               string.Equals(model.SpecificHost, entity.SpecificHost, StringComparison.OrdinalIgnoreCase);
    }
}