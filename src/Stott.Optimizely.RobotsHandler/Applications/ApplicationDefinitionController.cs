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

        return CreateSafeJsonResult(apps);
    }
}
