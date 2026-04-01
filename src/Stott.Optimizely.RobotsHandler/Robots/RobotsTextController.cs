namespace Stott.Optimizely.RobotsHandler.Robots;

using System;
using System.Threading.Tasks;
using EPiServer.Applications;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Stott.Optimizely.RobotsHandler.Extensions;

public sealed class RobotsTextController(
     IApplicationResolver applicationResolver,
     IRobotsContentService service,
     ILogger<RobotsTextController> logger) : Controller
{
    [HttpGet]
    [Route("robots.txt")]
    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        try
        {
            var application = await applicationResolver.GetByContextAsync();
            var robotsContent = service.GetRobotsContent(application?.Name, Request?.Host.Value);

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
            logger.LogError(exception, "Failed to load the robots.txt for the current site.");
            throw;
        }
    }
}