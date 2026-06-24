using System;
using System.Threading.Tasks;

using EPiServer.Applications;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Stott.Optimizely.RobotsHandler.Extensions;

namespace Stott.Optimizely.RobotsHandler.Llms;

public sealed class LlmsTextController(
    IApplicationResolver applicationResolver,
    ILlmsContentService service,
    ILogger<LlmsTextController> logger) : Controller
{
    [HttpGet]
    [Route("llms.txt")]
    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        try
        {
            var application = await applicationResolver.GetByContextAsync();
            var llmsContent = service.GetLlmsContent(application?.Name, Request?.Host.Value);
            if (string.IsNullOrWhiteSpace(llmsContent))
            {
                logger.LogWarning("The llms.txt content is empty for the current site.");
                return NotFound();
            }

            // Set a low cache duration, but not zero to ensure the CDN protects against DDOS attacks
            Response.Headers.AddOrUpdateHeader("Cache-Control", "public, max-age=300");

            return new ContentResult
            {
                Content = llmsContent,
                ContentType = "text/plain; charset=utf-8",
                StatusCode = 200
            };
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to load the llms.txt for the current site.");
            throw;
        }
    }
}