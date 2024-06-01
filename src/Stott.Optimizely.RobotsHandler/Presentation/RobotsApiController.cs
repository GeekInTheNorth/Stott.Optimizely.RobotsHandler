namespace Stott.Optimizely.RobotsHandler.Presentation;

using System;
using System.Linq;
using System.Net;
using System.Text.Json;

using EPiServer.Web;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Stott.Optimizely.RobotsHandler.Common;
using Stott.Optimizely.RobotsHandler.Extensions;
using Stott.Optimizely.RobotsHandler.Presentation.ViewModels;
using Stott.Optimizely.RobotsHandler.Services;

[ApiExplorerSettings(IgnoreApi = true)]
[Authorize(Policy = RobotsConstants.AuthorizationPolicy)]
public sealed class RobotsApiController : Controller
{
    private readonly IRobotsContentService _service;

    private readonly ISiteDefinitionRepository _siteRepository;

    private readonly ILogger<RobotsApiController> _logger;

    public RobotsApiController(
        IRobotsContentService service,
        ISiteDefinitionRepository siteRepository,
        ILogger<RobotsApiController> logger)
    {
        _service = service;
        _siteRepository = siteRepository;
        _logger = logger;
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
    public IActionResult Save(SaveRobotsModel formSubmitModel)
    {
        try
        {
            _service.SaveRobotsContent(formSubmitModel);

            return new OkResult();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to save robots.txt content for {siteName}", formSubmitModel.SiteName);
            return new ContentResult
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Content = exception.Message,
                ContentType = "text/plain"
            };
        }
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