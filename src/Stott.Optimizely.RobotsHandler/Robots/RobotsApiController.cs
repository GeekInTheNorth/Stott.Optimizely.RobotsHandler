namespace Stott.Optimizely.RobotsHandler.Robots;

using System;
using System.Net;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Stott.Optimizely.RobotsHandler.Common;

[ApiExplorerSettings(IgnoreApi = true)]
[Authorize(Policy = RobotsConstants.AuthorizationPolicy)]
public sealed class RobotsApiController(
    IRobotsContentService service,
    ILogger<RobotsApiController> logger) : BaseApiController
{
    [HttpGet]
    [Route("/stott.robotshandler/api/robots/list/")]
    public IActionResult ApiList()
    {
        var model = new RobotsListViewModel
        {
            List = service.GetAll()
        };

        return CreateSafeJsonResult(model);
    }

    [HttpGet]
    [Route("/stott.robotshandler/api/robots/[action]")]
    public IActionResult Details(string? id, string? appId)
    {
        if (!Guid.TryParse(id, out var robotsId))
        {
            throw new ArgumentException("Id cannot be parsed as a valid GUID.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(appId))
        {
            throw new ArgumentException("AppId has not been provided.", nameof(appId));
        }

        var model = Guid.Empty.Equals(robotsId) ? service.GetDefault(appId) : service.Get(robotsId);

        return CreateSafeJsonResult(model);
    }

    [HttpPost]
    [Route("/stott.robotshandler/api/robots/[action]")]
    public IActionResult Save(SaveRobotsModel formSubmitModel)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(formSubmitModel.AppId))
            {
                return new ContentResult
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Content = $"{nameof(formSubmitModel.AppId)} does not have a value.",
                    ContentType = "text/plain"
                };
            }

            if (service.DoesConflictExists(formSubmitModel))
            {
                return new ContentResult
                {
                    StatusCode = (int)HttpStatusCode.Conflict,
                    Content = "A robots configuration already exists for this site and host combination.",
                    ContentType = "text/plain"
                };
            }
            service.Save(formSubmitModel);

            return new OkResult();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to save robots.txt content for {siteName}", formSubmitModel.AppName);
            return new ContentResult
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Content = exception.Message,
                ContentType = "text/plain"
            };
        }
    }

    [HttpDelete]
    [Route("/stott.robotshandler/api/robots/[action]/{id}")]
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

            service.Delete(id);

            return new OkResult();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to delete this robots configuration.");
            return new ContentResult
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Content = exception.Message,
                ContentType = "text/plain"
            };
        }
    }
}