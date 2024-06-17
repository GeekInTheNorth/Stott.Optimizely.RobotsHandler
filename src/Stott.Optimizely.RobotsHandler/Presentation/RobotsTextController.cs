namespace Stott.Optimizely.RobotsHandler.Presentation;

using System;

using EPiServer.Web;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Stott.Optimizely.RobotsHandler.Extensions;
using Stott.Optimizely.RobotsHandler.Services;

public sealed class RobotsTextController : Controller
{
    private readonly IRobotsContentService _service;

    private readonly ILogger<RobotsTextController> _logger;

    public RobotsTextController(IRobotsContentService service, ILogger<RobotsTextController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    [Route("robots.txt")]
    [AllowAnonymous]
    public IActionResult Index()
    {
        try
        {
            var robotsContent = _service.GetRobotsContent(SiteDefinition.Current.Id, Request.Host.Value);

            // Set a low cache duration, but not zero to ensure the CDN protects against DDOS attacks
            Response.Headers.AddOrUpdateHeader("Cache-Control", "public, max-age=300");

            return new ContentResult
            {
                Content = robotsContent,
                ContentType = "text/plain",
                StatusCode = 200
            };
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to load the robots.txt for the current site.");
            throw;
        }
    }
}