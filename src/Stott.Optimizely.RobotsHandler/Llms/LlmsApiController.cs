using System;
using System.Net;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Stott.Optimizely.RobotsHandler.Common;

namespace Stott.Optimizely.RobotsHandler.Llms;

[ApiExplorerSettings(IgnoreApi = true)]
[Authorize(Policy = RobotsConstants.AuthorizationPolicy)]
public sealed class LlmsApiController : BaseApiController
{
    private readonly ILlmsContentService _service;

    private readonly ILogger<LlmsApiController> _logger;

    public LlmsApiController(
        ILlmsContentService service, 
        ILogger<LlmsApiController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    [Route("/stott.robotshandler/api/llms/list/")]
    public IActionResult ApiList()
    {
        var model = new LlmsListViewModel
        {
            List = _service.GetAll()
        };

        return CreateSafeJsonResult(model);
    }

    [HttpGet]
    [Route("/stott.robotshandler/api/llms/[action]")]
    public IActionResult Details(string id, string siteId)
    {
        if (!Guid.TryParse(id, out var llmsId))
        {
            throw new ArgumentException("Id cannot be parsed as a valid GUID.", nameof(id));
        }

        if (!Guid.TryParse(siteId, out var llmsSiteId) || Guid.Empty.Equals(llmsSiteId))
        {
            throw new ArgumentException("SiteId cannot be parsed as a valid GUID.", nameof(siteId));
        }

        var model = Guid.Empty.Equals(llmsId) ? _service.GetDefault(llmsSiteId) : _service.Get(llmsId);

        return CreateSafeJsonResult(model);
    }

    [HttpPost]
    [Route("/stott.robotshandler/api/llms/[action]")]
    public IActionResult Save(SaveLlmsModel formSubmitModel)
    {
        try
        {
            if (_service.DoesConflictExists(formSubmitModel))
            {
                return new ContentResult
                {
                    StatusCode = (int)HttpStatusCode.Conflict,
                    Content = "An LLMS configuration already exists for this site and host combination.",
                    ContentType = "text/plain"
                };
            }
            _service.Save(formSubmitModel);

            return new OkResult();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to save llms.txt content for {siteName}", formSubmitModel.SiteName);
            return new ContentResult
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Content = exception.Message,
                ContentType = "text/plain"
            };
        }
    }

    [HttpDelete]
    [Route("/stott.robotshandler/api/llms/[action]/{id}")]
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

            _service.Delete(id);

            return new OkResult();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to delete this llms configuration.");
            return new ContentResult
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Content = exception.Message,
                ContentType = "text/plain"
            };
        }
    }
}