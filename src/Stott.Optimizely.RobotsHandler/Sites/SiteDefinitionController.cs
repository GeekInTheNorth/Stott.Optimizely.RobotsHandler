using System.Linq;

using EPiServer.Web;

using Microsoft.AspNetCore.Mvc;

using Stott.Optimizely.RobotsHandler.Common;
using Stott.Optimizely.RobotsHandler.Extensions;

namespace Stott.Optimizely.RobotsHandler.Sites;

public sealed class SiteDefinitionController : BaseApiController
{
    private readonly ISiteDefinitionRepository _siteRepository;

    public SiteDefinitionController(ISiteDefinitionRepository siteRepository)
    {
        _siteRepository = siteRepository;
    }

    [HttpGet]
    [Route("/stott.robotshandler/api/[action]")]
    public IActionResult Sites()
    {
        var sites = _siteRepository.List()
                                   .Select(x => new SiteViewModel
                                   {
                                       SiteId = x.Id,
                                       SiteName = x.Name,
                                       AvailableHosts = x.Hosts.ToHostSummaries().ToList()
                                   })
                                   .ToList();

        return CreateSafeJsonResult(sites);
    }
}
