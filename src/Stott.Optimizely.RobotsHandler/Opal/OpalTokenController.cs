using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Stott.Optimizely.RobotsHandler.Common;

namespace Stott.Optimizely.RobotsHandler.Opal;

[ApiExplorerSettings(IgnoreApi = true)]
[Authorize(Policy = RobotsConstants.AuthorizationPolicy)]
public class OpalTokenController : BaseApiController
{
    private readonly ILogger<OpalTokenController> _logger;

    public OpalTokenController(ILogger<OpalTokenController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    [Route("/stott.robotshandler/api/opal-tokens/[action]")]
    public IActionResult List()
    {
        return CreateSafeJsonResult(new List<object>());
    }

    [HttpPost]
    [Route("/stott.robotshandler/api/opal-tokens/[action]")]
    public IActionResult Save()
    {
        return new OkResult();
    }

    [HttpDelete]
    [Route("/stott.robotshandler/api/opal-tokens/[action]")]
    public IActionResult Delete(Guid id)
    {
        try
        {
            if (Guid.Empty.Equals(id))
            {
                return new ContentResult
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Content = "Id must not be empty.",
                    ContentType = "text/plain"
                };
            }

            // _service.Delete(id);

            return new OkResult();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to delete this token.");
            return new ContentResult
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Content = exception.Message,
                ContentType = "text/plain"
            };
        }
    }
}