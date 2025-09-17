using System;
using System.Net;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Stott.Optimizely.RobotsHandler.Common;

namespace Stott.Optimizely.RobotsHandler.Opal;

[ApiExplorerSettings(IgnoreApi = true)]
[Authorize(Policy = RobotsConstants.AuthorizationPolicy)]
public class OpalTokenController : BaseApiController
{
    private readonly IOpalTokenRepository _repository;

    private readonly ILogger<OpalTokenController> _logger;

    public OpalTokenController(IOpalTokenRepository repository, ILogger<OpalTokenController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    [Route("/stott.robotshandler/api/opal-tokens/[action]")]
    public IActionResult List()
    {
        try
        {
            var tokens = _repository.List();
            if (tokens is null)
            {
                return CreateSafeJsonResult(Array.Empty<TokenModel>());
            }

            return CreateSafeJsonResult(tokens);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to retrieve token list.");
            return new ContentResult
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Content = "Failed to retrieve tokens.",
                ContentType = "text/plain"
            };
        }
    }

    [HttpPost]
    [Route("/stott.robotshandler/api/opal-tokens/[action]")]
    public IActionResult Save(TokenModel model)
    {
        try
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                return new ContentResult
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Content = "Token name is required.",
                    ContentType = "text/plain"
                };
            }

            // For new tokens, ensure a token value is provided
            if (model.Id == Guid.Empty && string.IsNullOrWhiteSpace(model.Token))
            {
                return new ContentResult
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Content = "Token value is required for new tokens.",
                    ContentType = "text/plain"
                };
            }

            _repository.Save(model);

            return new OkResult();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to save token.");
            return new ContentResult
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Content = "Failed to save token.",
                ContentType = "text/plain"
            };
        }
    }

    [HttpDelete]
    [Route("/stott.robotshandler/api/opal-tokens/[action]/{id}")]
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

            _repository.Delete(id);

            return new OkResult();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to delete token with ID: {TokenId}", id);
            return new ContentResult
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Content = "Failed to delete token.",
                ContentType = "text/plain"
            };
        }
    }
}