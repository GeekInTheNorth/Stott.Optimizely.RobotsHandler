using System;
using System.Collections.Generic;
using System.Linq;

using Stott.Optimizely.RobotsHandler.Applications;
using Stott.Optimizely.RobotsHandler.Models;

namespace Stott.Optimizely.RobotsHandler.Llms;

public class DefaultLlmsContentService(
    IApplicationDefinitionService appService, 
    ILlmsContentRepository llmsContentRepository) : ILlmsContentService
{
    public void Delete(Guid id)
    {
        if (Guid.Empty.Equals(id))
        {
            return;
        }

        llmsContentRepository.Delete(id);
    }

    public bool DoesConflictExists(SaveLlmsModel model)
    {
        var existingConfigurations = llmsContentRepository.GetAll() ?? [];
        return existingConfigurations.Any(x => IsConflict(model, x));
    }

    public ApplicationLlmsViewModel Get(Guid id)
    {
        var robotRecord = llmsContentRepository.Get(id);
        if (robotRecord == null)
        {
            throw new RobotsEntityNotFoundException(id);
        }

        var applications = appService.GetAllApplicationsAsync().GetAwaiter().GetResult();
        var application = applications.FirstOrDefault(x => string.Equals(x.AppId, robotRecord.AppId, StringComparison.OrdinalIgnoreCase));
        if (application == null)
        {
            throw new RobotsEntityNotFoundException($"Llms entity with id '{id}' not match any application definitions.");
        }

        return ToModel(robotRecord, application);
    }

    public IList<ApplicationLlmsViewModel> GetAll()
    {
        var allRecords = llmsContentRepository.GetAll();
        var applications = appService.GetAllApplicationsAsync().GetAwaiter().GetResult();
        var models = new List<ApplicationLlmsViewModel>();

        foreach (var robotRecord in allRecords)
        {
            var application = applications.FirstOrDefault(x => string.Equals(x.AppId, robotRecord.AppId));
            if (application != null)
            {
                models.Add(ToModel(robotRecord, application));
            }
        }

        return [.. models.OrderBy(x => x.AppName).ThenBy(x => x.SpecificHost)];
    }

    public ApplicationLlmsViewModel GetDefault(string? appId)
    {
        var application = appService.GetApplicationByIdAsync(appId).GetAwaiter().GetResult();
        if (application == null)
        {
            throw new ArgumentException($"{nameof(appId)} does not correlate to a known application.", nameof(appId));
        }

        return ToModel(application);
    }

    public string? GetDefaultLlmsContent()
    {
        return string.Empty;
    }

    public string? GetLlmsContent(string? appId, string? host)
    {
        var llmsEntries = llmsContentRepository.GetAllForSite(appId) ?? [];
        var matchingLlms = llmsEntries.FirstOrDefault(x => string.Equals(x.SpecificHost, host, StringComparison.OrdinalIgnoreCase)) ??
                           llmsEntries.FirstOrDefault(x => string.IsNullOrWhiteSpace(x.SpecificHost));

        return matchingLlms?.LlmsContent;
    }

    public void Save(SaveLlmsModel model)
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

        llmsContentRepository.Save(model);
    }

    private static ApplicationLlmsViewModel ToModel(LlmsTxtEntity entity, ApplicationViewModel application)
    {
        return new ApplicationLlmsViewModel
        {
            Id = entity.Id.ExternalId,
            AppId = entity.AppId,
            IsForWholeSite = entity.IsForWholeSite || string.IsNullOrWhiteSpace(entity.SpecificHost),
            SpecificHost = entity.SpecificHost,
            LlmsContent = entity.LlmsContent,
            AppName = application.AppName,
            AvailableHosts = application.AvailableHosts
        };
    }

    private ApplicationLlmsViewModel ToModel(ApplicationViewModel application)
    {
        return new ApplicationLlmsViewModel
        {
            Id = Guid.Empty,
            AppId = application.AppId,
            IsForWholeSite = true,
            LlmsContent = GetDefaultLlmsContent(),
            AppName = application.AppName,
            AvailableHosts = application.AvailableHosts
        };
    }

    private static bool IsConflict(SaveLlmsModel model, LlmsTxtEntity entity)
    {
        var modelHost = model.SpecificHost ?? string.Empty;
        var entityHost = entity.SpecificHost ?? string.Empty;

        return string.Equals(model.AppId, entity.AppId, StringComparison.OrdinalIgnoreCase) &&
               !Guid.Equals(model.Id, entity.Id.ExternalId) &&
               string.Equals(modelHost, entityHost, StringComparison.OrdinalIgnoreCase);
    }
}
