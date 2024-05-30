namespace Stott.Optimizely.RobotsHandler.Presentation;

using System;
using System.Net;
using System.Text.Json;

using EPiServer.Logging;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Stott.Optimizely.RobotsHandler.Common;
using Stott.Optimizely.RobotsHandler.Presentation.ViewModels;
using Stott.Optimizely.RobotsHandler.Services;

[ApiExplorerSettings(IgnoreApi = true)]
[Authorize(Policy = RobotsConstants.AuthorizationPolicy)]
public class RobotsApiController : Controller
{
    private readonly IRobotsContentService _service;

    private readonly ILogger _logger = LogManager.GetLogger(typeof(RobotsApiController));

    public RobotsApiController(IRobotsContentService service)
    {
        _service = service;
    }

    [HttpGet]
    [Route("/stott.robotshandler/api/list/")]
    public IActionResult ApiList()
    {
        var model = new RobotsListViewModel
        {
            List = _service.GetAll()
        };

        return CreateSafeJsonResult(model);
    }

    [HttpGet]
    [Route("/stott.robotshandler/api/[action]")]
    public IActionResult Details(string id, string siteId)
    {
        if (!Guid.TryParse(id, out var robotsId))
        {
            throw new ArgumentException("Id cannot be parsed as a valid GUID.", nameof(id));
        }

        if (!Guid.TryParse(siteId, out var robotsSiteId) || Guid.Empty.Equals(robotsSiteId))
        {
            throw new ArgumentException("SiteId cannot be parsed as a valid GUID.", nameof(siteId));
        }

        var model = Guid.Empty.Equals(robotsId) ? _service.GetDefault(robotsSiteId) : _service.Get(robotsId);

        return CreateSafeJsonResult(model);
    }

    [HttpPost]
    [Route("/stott.robotshandler/api/[action]")]
    public IActionResult Save(RobotsEditViewModel formSubmitModel)
    {
        try
        {
            _service.SaveRobotsContent(formSubmitModel.SiteId, formSubmitModel.RobotsContent);

            return new OkResult();
        }
        catch (Exception exception)
        {
            _logger.Error($"Failed to save robots.txt content for {formSubmitModel.SiteName}", exception);
            return new ContentResult
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Content = exception.Message,
                ContentType = "text/plain"
            };
        }
    }

    private static IActionResult CreateSafeJsonResult<T>(T objectToSerialize)
    {
        var serializationOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var content = JsonSerializer.Serialize(objectToSerialize, serializationOptions);

        return new ContentResult
        {
            StatusCode = (int)HttpStatusCode.OK,
            ContentType = "application/json",
            Content = content
        };
    }
}