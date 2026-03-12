using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Stott.Optimizely.RobotsHandler.Common;

namespace Stott.Optimizely.RobotsHandler.Applications;

[ApiExplorerSettings(IgnoreApi = true)]
[Authorize(Policy = RobotsConstants.AuthorizationPolicy)]
public sealed class ApplicationDefinitionController(IApplicationDefinitionService appService) : BaseApiController
{
    [HttpGet]
    [Route("/stott.robotshandler/api/[action]")]
    public async Task<IActionResult> Applications()
    {
        var apps = await appService.GetAllApplicationsAsync();
        var allApps = apps.ToList();

        allApps.Insert(0, new ApplicationViewModel
        {
            AppName = "All Applications",
            AvailableHosts = ApplicationMapper.CreateHostSummaries("All Hosts")
        });

        return CreateSafeJsonResult(allApps);
    }
}
