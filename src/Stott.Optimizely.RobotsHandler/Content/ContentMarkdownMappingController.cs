using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Stott.Optimizely.RobotsHandler.Common;

namespace Stott.Optimizely.RobotsHandler.Content;

[ApiExplorerSettings(IgnoreApi = true)]
[Authorize(Policy = RobotsConstants.AuthorizationPolicy)]
public sealed class ContentMarkdownMappingController : BaseApiController
{
    private readonly IContentMarkdownMappingService _service;

    public ContentMarkdownMappingController(IContentMarkdownMappingService service)
    {
        _service = service;
    }

    [HttpGet]
    [Route("/stott.robotshandler/api/markdown/[action]")]
    public IActionResult Settings()
    {
        var model = _service.GetSettings();
        
        return CreateSafeJsonResult(model);
    }

    [HttpGet]
    [Route("/stott.robotshandler/api/markdown/[action]")]
    public IActionResult List()
    {
        var model = _service.List();

        return CreateSafeJsonResult(model);
    }

    [HttpGet]
    [Route("/stott.robotshandler/api/markdown/[action]")]
    public IActionResult Details(Guid? id)
    {
        if (id == null || id == Guid.Empty)
        {
            return BadRequest("A valid content type ID must be provided.");
        }

        var model = _service.Get(id.Value);

        return CreateSafeJsonResult(model);
    }
}