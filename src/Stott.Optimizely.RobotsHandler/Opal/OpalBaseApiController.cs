using System;
using System.Collections.Generic;
using System.Linq;

using Stott.Optimizely.RobotsHandler.Common;
using Stott.Optimizely.RobotsHandler.Opal.Models;

namespace Stott.Optimizely.RobotsHandler.Opal;

public abstract class OpalBaseApiController : BaseApiController
{
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