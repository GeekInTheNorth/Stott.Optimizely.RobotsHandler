using System;
using System.Net;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Stott.Optimizely.RobotsHandler.Common;

namespace Stott.Optimizely.RobotsHandler.Environments;

[ApiExplorerSettings(IgnoreApi = true)]
[Authorize(Policy = RobotsConstants.AuthorizationPolicy)]
public sealed class EnvironmentApiController(
    IEnvironmentRobotsService service,
    ILogger<EnvironmentApiController> logger) : BaseApiController
{
    [HttpGet]
    [Route("/stott.robotshandler/api/environment/[action]")]
    public IActionResult List()
    {
        var model = service.GetAll();

        return CreateSafeJsonResult(model);
    }

    [HttpPost]
    [Route("/stott.robotshandler/api/environment/[action]")]
    public IActionResult Save(EnvironmentRobotsModel model)
    {
        try
        {
            service.Save(model);

            return new OkResult();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to save robots.txt content for {environment}", model.EnvironmentName);
            return new ContentResult
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Content = exception.Message,
                ContentType = "text/plain"
            };
        }
    }
}
