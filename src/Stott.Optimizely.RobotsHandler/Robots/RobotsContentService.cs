using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Stott.Optimizely.RobotsHandler.Applications;
using Stott.Optimizely.RobotsHandler.Extensions;
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

        foreach (var site in applications)
        {
            if (!models.Any(x => string.Equals(x.AppId, site.AppId, StringComparison.OrdinalIgnoreCase) && x.IsForWholeSite))
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

        var applications = appService.GetAllApplicationsAsync().GetAwaiter().GetResult();
        var application = applications.FirstOrDefault(x => string.Equals(x.AppId, robotRecord.AppId, StringComparison.OrdinalIgnoreCase));
        if (application == null)
        {
            throw new RobotsEntityNotFoundException($"Robotes entity with id '{id}' not match an application definition.");
        }

        return ToModel(robotRecord, application);
    }

    public SiteRobotsViewModel GetDefault(string? appId)
    {
        var application = appService.GetApplicationByIdAsync(appId).GetAwaiter().GetResult();
        if (application == null)
        {
            throw new ArgumentException($"{nameof(appId)} does not correlate to a known application.", nameof(appId));
        }

        return ToModel(application);
    }

    public string? GetRobotsContent(string? appId, string? host)
    {
        var cleanedHost = host.GetSanitizedHostDomain();
        var robots = robotsContentRepository.GetAllForSite(appId) ?? [];
        var matchingRobots = robots.FirstOrDefault(x => string.Equals(x.SpecificHost, cleanedHost, StringComparison.OrdinalIgnoreCase)) ??
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

        var application = appService.GetApplicationByIdAsync(model.AppId).GetAwaiter().GetResult();
        if (application == null)
        {
            throw new ArgumentException($"{nameof(model)}.{nameof(model.AppId)} does not correlate to a known application.", nameof(model));
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

    private static SiteRobotsViewModel ToModel(RobotsEntity robotsEntity, ApplicationViewModel application)
    {
        return new SiteRobotsViewModel
        {
            Id = robotsEntity.Id.ExternalId,
            AppId = robotsEntity.AppId,
            IsForWholeSite = robotsEntity.IsForWholeSite || string.IsNullOrWhiteSpace(robotsEntity.SpecificHost),
            SpecificHost = robotsEntity.SpecificHost,
            RobotsContent = robotsEntity.RobotsContent,
            AppName = application.AppName,
            AvailableHosts = application.AvailableHosts,
            CanDelete = true
        };
    }

    private SiteRobotsViewModel ToModel(ApplicationViewModel application)
    {
        return new SiteRobotsViewModel
        {
            Id = Guid.Empty,
            AppId = application.AppId,
            IsForWholeSite = true,
            RobotsContent = GetDefaultRobotsContent(),
            AppName = application.AppName,
            AvailableHosts = application.AvailableHosts
        };
    }

    private static bool IsConflict(SaveRobotsModel model, RobotsEntity entity)
    {
        var modelHost = model.SpecificHost.GetSanitizedHostDomain() ?? string.Empty;
        var entityHost = entity.SpecificHost.GetSanitizedHostDomain() ?? string.Empty;

        return string.Equals(model.AppId, entity.AppId, StringComparison.OrdinalIgnoreCase) && 
               !Guid.Equals(model.Id, entity.Id.ExternalId) &&
               string.Equals(modelHost, entityHost, StringComparison.OrdinalIgnoreCase);
    }
}