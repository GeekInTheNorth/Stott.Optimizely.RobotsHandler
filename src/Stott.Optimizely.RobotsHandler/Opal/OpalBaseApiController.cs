using System;
using System.Collections.Generic;
using System.Linq;

using EPiServer.Web;

using Stott.Optimizely.RobotsHandler.Common;
using Stott.Optimizely.RobotsHandler.Extensions;
using Stott.Optimizely.RobotsHandler.Opal.Models;

namespace Stott.Optimizely.RobotsHandler.Opal;

public abstract class OpalBaseApiController : BaseApiController
{
    private readonly ISiteDefinitionResolver _siteDefinitionResolver;

    protected OpalBaseApiController(ISiteDefinitionResolver siteDefinitionResolver)
    {
        _siteDefinitionResolver = siteDefinitionResolver;
    }

    protected static IEnumerable<OpalSiteContentModel> ConvertToModels<TContent>(IList<TContent> siteModels, Func<TContent, string> contentSelector)
        where TContent : ISiteContentViewModel
    {
        if (siteModels is null)
        {
            yield break;
        }

        var specifiedHosts = GetSpecificHosts<TContent>(siteModels);

        foreach (var siteModel in siteModels)
        {
            if (!string.IsNullOrWhiteSpace(siteModel.SpecificHost))
            {
                yield return new OpalSiteContentModel
                {
                    Id = siteModel.Id,
                    SpecificHost = siteModel.SpecificHost,
                    SiteName = siteModel.SiteName,
                    Content = contentSelector(siteModel)
                };

                continue;
            }

            var availableHosts = siteModel.AvailableHosts.Where(x => !string.IsNullOrWhiteSpace(x.HostName)).Select(x => x.HostName).ToList();
            foreach (var availableHost in availableHosts)
            {
                if (!specifiedHosts.Any(x => string.Equals(x, availableHost, StringComparison.OrdinalIgnoreCase)))
                {
                    yield return new OpalSiteContentModel
                    {
                        Id = siteModel.Id,
                        SpecificHost = availableHost,
                        SiteName = siteModel.SiteName,
                        Content = contentSelector(siteModel)
                    };
                }
            }
        }
    }

    protected static OpalSiteContentModel ConvertToModel<TContent>(TContent viewModel, string specificHost, Func<TContent, string> contentSelector)
        where TContent : ISiteContentViewModel
    {
        return new OpalSiteContentModel
        {
            Id = viewModel.Id,
            SpecificHost = specificHost,
            SiteName = viewModel.SiteName,
            Content = contentSelector(viewModel)
        };
    }

    protected TModel GetEmptySiteModel<TModel>(string hostName)
        where TModel : ISiteContentViewModel
    {
        var siteDefinition = _siteDefinitionResolver.GetByHostname(hostName, false, out var matchedHost);
        if (siteDefinition is null)
        {
            return default;
        }

        var model = Activator.CreateInstance<TModel>();
        model.Id = Guid.Empty;
        model.SiteId = siteDefinition.Id;
        model.IsForWholeSite = false;
        model.SpecificHost = hostName;
        model.SiteName = siteDefinition.Name;
        model.AvailableHosts = siteDefinition.Hosts.ToHostSummaries().ToList();

        return model;
    }

    private static IList<string> GetSpecificHosts<TContent>(IList<TContent> models)
        where TContent : ISiteContentViewModel
    {
        if (models is null)
        {
            return new List<string>();
        }

        return models.Where(x => !string.IsNullOrWhiteSpace(x.SpecificHost)).Select(x => x.SpecificHost).ToList();
    }
}