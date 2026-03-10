using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EPiServer.Web;

using Stott.Optimizely.RobotsHandler.Applications;
using Stott.Optimizely.RobotsHandler.Models;

namespace Stott.Optimizely.RobotsHandler.Robots;

public sealed class RobotsContentService(
    IApplicationDefinitionService appService,
    IRobotsContentRepository robotsContentRepository) : IRobotsContentService
{
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
        var applications = appService.GetAllApplicationsAsync().GetAwaiter().GetResult();
        var models = new List<SiteRobotsViewModel>();

        foreach (var robotRecord in robotRecords)
        {
            var site = applications.FirstOrDefault(x => string.Equals(x.AppId, robotRecord.AppId, StringComparison.OrdinalIgnoreCase));
            if (site != null)
            {
                models.Add(ToModel(robotRecord, site));
            }
        }

        foreach (var site in sites)
        {
            if (!models.Any(x => x.AppId == site.Id && x.IsForWholeSite))
            {
                models.Add(ToModel(site));
            }
        }

        return models.OrderBy(x => x.AppName).ThenBy(x => x.SpecificHost).ToList();
    }

    public SiteRobotsViewModel Get(Guid id)
    {
        var robotRecord = robotsContentRepository.Get(id);
        if (robotRecord == null)
        {
            throw new RobotsEntityNotFoundException(id);
        }

        var sites = appService.List();
        var site = sites.FirstOrDefault(x => x.Id.Equals(robotRecord.SiteId));
        if (site == null)
        {
            throw new RobotsEntityNotFoundException($"Robotes entity with id '{id}' not match a site definition.");
        }

        return ToModel(robotRecord, site);
    }

    public SiteRobotsViewModel GetDefault(Guid siteId)
    {
        var site = appService.Get(siteId);
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
        if (Guid.Empty.Equals(model.AppId))
        {
            throw new ArgumentException($"{nameof(model)}.{nameof(model.AppId)} must not be null or empty.", nameof(model));
        }

        var existingSite = appService.Get(model.AppId);
        if (existingSite == null)
        {
            throw new ArgumentException($"{nameof(model)}.{nameof(model.AppId)} does not correlate to a known site.", nameof(model));
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
            AppId = robotsEntity.SiteId,
            IsForWholeSite = robotsEntity.IsForWholeSite || string.IsNullOrWhiteSpace(robotsEntity.SpecificHost),
            SpecificHost = robotsEntity.SpecificHost,
            RobotsContent = robotsEntity.RobotsContent,
            AppName = siteDefinition.Name,
            AvailableHosts = siteDefinition.Hosts.ToHostSummaries().ToList(),
            CanDelete = true
        };
    }

    private SiteRobotsViewModel ToModel(SiteDefinition siteDefinition)
    {
        return new SiteRobotsViewModel
        {
            Id = Guid.Empty,
            AppId = siteDefinition.Id,
            IsForWholeSite = true,
            RobotsContent = GetDefaultRobotsContent(),
            AppName = siteDefinition.Name,
            AvailableHosts = siteDefinition.Hosts.ToHostSummaries().ToList()
        };
    }

    private static bool IsConflict(SaveRobotsModel model, RobotsEntity entity)
    {
        var modelHost = model.SpecificHost ?? string.Empty;
        var entityHost = entity.SpecificHost ?? string.Empty;

        return Equals(model.AppId, entity.SiteId) && !Equals(model.Id, entity.Id.ExternalId) &&
               string.Equals(modelHost, entityHost, StringComparison.OrdinalIgnoreCase);
    }
}