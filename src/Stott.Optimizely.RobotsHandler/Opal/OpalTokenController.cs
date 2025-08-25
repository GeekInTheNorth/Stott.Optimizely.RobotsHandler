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
        var tokens = _repository.List();

        foreach(var token in tokens)
        {
            // update token.Token so that only the first 6 characters of the token and an ellipse are returned
            token.Token = token.Token.Length > 6 ? token.Token[..6] + "..." : token.Token;
        }

        return CreateSafeJsonResult(tokens);
    }

    [HttpPost]
    [Route("/stott.robotshandler/api/opal-tokens/[action]")]
    public IActionResult Save(TokenModel model)
    {
        _repository.Save(model);

        return new OkResult();
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
            _logger.LogError(exception, "Failed to delete this token.");
            return new ContentResult
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Content = exception.Message,
                ContentType = "text/plain"
            };
        }
    }
}