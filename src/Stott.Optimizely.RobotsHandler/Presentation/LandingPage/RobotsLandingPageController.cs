namespace Stott.Optimizely.RobotsHandler.Presentation.LandingPage;

using System;
using System.Reflection;

using EPiServer.Framework.ClientResources;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stott.Optimizely.RobotsHandler.Common;
using Stott.Optimizely.RobotsHandler.Models;
using Stott.Security.Optimizely.Features.StaticFile;

[Authorize(Policy = RobotsConstants.AuthorizationPolicy)]
public sealed class RobotsLandingPageController : Controller
{
    private readonly ICspNonceService _cspNonceService;

    private readonly IStaticFileResolver _staticFileResolver;

    public RobotsLandingPageController(ICspNonceService cspNonceService, IStaticFileResolver staticFileResolver)
    {
        _cspNonceService = cspNonceService;
        _staticFileResolver = staticFileResolver;
    }

    [HttpGet]
    [Route("/stott.robotshandler/administration/")]
    public IActionResult Index()
    {
        var model = new LandingPageViewModel
        {
            ApplicationName = "Stott Robots Handler",
            ApplicationVersion = GetApplicationVersion(),
            JavaScriptFile = $"/stott.robotshandler/static/{_staticFileResolver.GetJavaScriptFileName()}",
            CssFile = $"/stott.robotshandler/static/{_staticFileResolver.GetStyleSheetFileName()}",
            Nonce = GetNonce()
        };

        return View(model);
    }

    [HttpGet]
    [Route("/stott.robotshandler/static/{staticFileName}")]
    public IActionResult ApplicationStaticFile(string staticFileName)
    {
        var fileBytes = _staticFileResolver.GetFileContent(staticFileName);
        var mimeType = _staticFileResolver.GetFileMimeType(staticFileName);

        if (fileBytes.Length == 0)
        {
            return NotFound("The requested file does not exist.");
        }

        return File(fileBytes, mimeType);
    }

    private static string GetApplicationVersion()
    {
        var assembly = Assembly.GetAssembly(typeof(RobotsEntity));
        var assemblyName = assembly?.GetName();

        return $"v{assemblyName?.Version}";
    }

    private string GetNonce()
    {
        try
        {
            return _cspNonceService.GetNonce();
        }
        catch (Exception)
        {
            return null;
        }
    }
}