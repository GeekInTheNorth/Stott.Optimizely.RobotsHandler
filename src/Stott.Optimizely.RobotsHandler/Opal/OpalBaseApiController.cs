using System;
using System.Collections.Generic;
using System.Linq;

using EPiServer.Applications;

using Stott.Optimizely.RobotsHandler.Applications;
using Stott.Optimizely.RobotsHandler.Common;
using Stott.Optimizely.RobotsHandler.Opal.Models;

namespace Stott.Optimizely.RobotsHandler.Opal;

public abstract class OpalBaseApiController(IApplicationResolver applicationResolver) : BaseApiController
{
    protected static IEnumerable<OpalSiteContentModel> ConvertToModels<TContent>(IList<TContent> siteModels, Func<TContent, string?> contentSelector)
        where TContent : IApplicationContentViewModel
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
                    SiteName = siteModel.AppName,
                    Content = contentSelector(siteModel)
                };

                continue;
            }

            var availableHosts = siteModel.AvailableHosts?.Where(x => !string.IsNullOrWhiteSpace(x.HostName)).Select(x => x.HostName!).ToList() ?? [];
            foreach (var availableHost in availableHosts)
            {
                if (!specifiedHosts.Any(x => string.Equals(x, availableHost, StringComparison.OrdinalIgnoreCase)))
                {
                    yield return new OpalSiteContentModel
                    {
                        Id = siteModel.Id,
                        SpecificHost = availableHost,
                        SiteName = siteModel.AppName,
                        Content = contentSelector(siteModel)
                    };
                }
            }
        }
    }

    protected static OpalSiteContentModel ConvertToModel<TContent>(TContent viewModel, string specificHost, Func<TContent, string?> contentSelector)
        where TContent : IApplicationContentViewModel
    {
        return new OpalSiteContentModel
        {
            Id = viewModel.Id,
            SpecificHost = specificHost,
            SiteName = viewModel.AppName,
            Content = contentSelector(viewModel)
        };
    }

    protected TModel? GetEmptySiteModel<TModel>(string hostName)
        where TModel : IApplicationContentViewModel
    {
        var applicationDefinition = applicationResolver.GetByHostname(hostName, false);
        if (applicationDefinition?.Application is null)
        {
            return default;
        }

        var model = Activator.CreateInstance<TModel>();
        model.Id = Guid.Empty;
        model.AppId = applicationDefinition.Application?.Name;
        model.IsForWholeSite = false;
        model.SpecificHost = hostName;
        model.AppName = applicationDefinition.Application?.DisplayName;
        model.AvailableHosts = GetHostViewModels(applicationDefinition.Application).ToList();

        return model;
    }

    private static IList<string> GetSpecificHosts<TContent>(IList<TContent> models)
        where TContent : IApplicationContentViewModel
    {
        if (models is null)
        {
            return [];
        }

        return models.Where(x => !string.IsNullOrWhiteSpace(x.SpecificHost)).Select(x => x.SpecificHost!).ToList();
    }

    private static IEnumerable<HostViewModel> GetHostViewModels(Application? application)
    {
        if (application is Website website)
        {
            return ApplicationMapper.CreateHostSummaries(website.Hosts);
        }

        if (application is RemoteWebsite remoteWebsite)
        {
            return ApplicationMapper.CreateHostSummaries(remoteWebsite.Hosts);
        }

        return Enumerable.Empty<HostViewModel>();
    }
}