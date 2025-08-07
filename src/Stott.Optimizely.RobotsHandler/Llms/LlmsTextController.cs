using System;

using EPiServer.Web;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Stott.Optimizely.RobotsHandler.Extensions;

namespace Stott.Optimizely.RobotsHandler.Llms;

public sealed class LlmsTextController : Controller
{
    private readonly ILlmsContentService _service;

    private readonly ILogger<LlmsTextController> _logger;

    public LlmsTextController(ILlmsContentService service, ILogger<LlmsTextController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    [Route("llms.txt")]
    [AllowAnonymous]
    public IActionResult Index()
    {
        try
        {
            var llmsContent = _service.GetLlmsContent(SiteDefinition.Current.Id, Request.Host.Value);

            if (string.IsNullOrWhiteSpace(llmsContent))
            {
                _logger.LogWarning("The llms.txt content is empty for the current site.");
                return NotFound();
            }

            // Set a low cache duration, but not zero to ensure the CDN protects against DDOS attacks
            Response.Headers.AddOrUpdateHeader("Cache-Control", "public, max-age=300");

            return new ContentResult
            {
                Content = llmsContent,
                ContentType = "text/plain",
                StatusCode = 200
            };
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to load the llms.txt for the current site.");
            throw;
        }
    }
}