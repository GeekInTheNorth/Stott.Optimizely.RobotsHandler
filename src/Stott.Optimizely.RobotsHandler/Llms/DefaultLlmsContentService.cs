using System;
using System.Collections.Generic;
using System.Linq;

using EPiServer.Web;

using Stott.Optimizely.RobotsHandler.Extensions;
using Stott.Optimizely.RobotsHandler.Models;

namespace Stott.Optimizely.RobotsHandler.Llms;

public class DefaultLlmsContentService : ILlmsContentService
{
    private readonly ISiteDefinitionRepository siteDefinitionRepository;

    private readonly ILlmsContentRepository llmsContentRepository;

    public DefaultLlmsContentService(
        ISiteDefinitionRepository siteDefinitionRepository,
        ILlmsContentRepository llmsContentRepository)
    {
        this.siteDefinitionRepository = siteDefinitionRepository;
        this.llmsContentRepository = llmsContentRepository;
    }

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
        var existingConfigurations = llmsContentRepository.GetAll() ?? new List<LlmsTxtEntity>(0);
        return existingConfigurations.Any(x => IsConflict(model, x));
    }

    public SiteLlmsViewModel Get(Guid id)
    {
        var robotRecord = llmsContentRepository.Get(id);
        if (robotRecord == null)
        {
            throw new RobotsEntityNotFoundException(id);
        }

        var sites = siteDefinitionRepository.List();
        var site = sites.FirstOrDefault(x => x.Id.Equals(robotRecord.SiteId));
        if (site == null)
        {
            throw new RobotsEntityNotFoundException($"Llms entity with id '{id}' not match any site definitions.");
        }

        return ToModel(robotRecord, site);
    }

    public IList<SiteLlmsViewModel> GetAll()
    {
        var allRecords = llmsContentRepository.GetAll();
        var sites = siteDefinitionRepository.List();
        var models = new List<SiteLlmsViewModel>();

        foreach (var robotRecord in allRecords)
        {
            var site = sites.FirstOrDefault(x => x.Id.Equals(robotRecord.SiteId));
            if (site != null)
            {
                models.Add(ToModel(robotRecord, site));
            }
        }

        return models.OrderBy(x => x.SiteName).ThenBy(x => x.SpecificHost).ToList();
    }

    public SiteLlmsViewModel GetDefault(Guid siteId)
    {
        var site = siteDefinitionRepository.Get(siteId);
        if (site == null)
        {
            throw new ArgumentException($"{nameof(siteId)} does not correlate to a known site.", nameof(siteId));
        }

        return ToModel(site);
    }

    public string GetDefaultLlmsContent()
    {
        return string.Empty;
    }

    public string GetLlmsContent(Guid siteId, string host)
    {
        var llmsEntries = llmsContentRepository.GetAllForSite(siteId) ?? new List<LlmsTxtEntity>(0);
        var matchingLlms = llmsEntries.FirstOrDefault(x => string.Equals(x.SpecificHost, host, StringComparison.OrdinalIgnoreCase)) ??
                           llmsEntries.FirstOrDefault(x => string.IsNullOrWhiteSpace(x.SpecificHost));

        return matchingLlms?.LlmsContent;
    }

    public void Save(SaveLlmsModel model)
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

        llmsContentRepository.Save(model);
    }

    private static SiteLlmsViewModel ToModel(LlmsTxtEntity entity, SiteDefinition siteDefinition)
    {
        return new SiteLlmsViewModel
        {
            Id = entity.Id.ExternalId,
            SiteId = entity.SiteId,
            IsForWholeSite = entity.IsForWholeSite || string.IsNullOrWhiteSpace(entity.SpecificHost),
            SpecificHost = entity.SpecificHost,
            LlmsContent = entity.LlmsContent,
            SiteName = siteDefinition.Name,
            AvailableHosts = siteDefinition.Hosts.ToHostSummaries().ToList()
        };
    }

    private SiteLlmsViewModel ToModel(SiteDefinition siteDefinition)
    {
        return new SiteLlmsViewModel
        {
            Id = Guid.Empty,
            SiteId = siteDefinition.Id,
            IsForWholeSite = true,
            LlmsContent = GetDefaultLlmsContent(),
            SiteName = siteDefinition.Name,
            AvailableHosts = siteDefinition.Hosts.ToHostSummaries().ToList()
        };
    }

    private static bool IsConflict(SaveLlmsModel model, LlmsTxtEntity entity)
    {
        var modelHost = model.SpecificHost ?? string.Empty;
        var entityHost = entity.SpecificHost ?? string.Empty;

        return Equals(model.SiteId, entity.SiteId) && !Equals(model.Id, entity.Id.ExternalId) &&
               string.Equals(modelHost, entityHost, StringComparison.OrdinalIgnoreCase);
    }
}
