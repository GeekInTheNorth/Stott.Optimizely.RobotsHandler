using System;
using System.Net;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Stott.Optimizely.RobotsHandler.Common;

namespace Stott.Optimizely.RobotsHandler.QueryRules;

[ApiExplorerSettings(IgnoreApi = true)]
[Authorize(Policy = RobotsConstants.AuthorizationPolicy)]
public sealed class QueryRulesApiController(
    IQueryRulesService service,
    ILogger<QueryRulesApiController> logger) : BaseApiController
{
    [HttpGet]
    [Route("/stott.robotshandler/api/query-rules/list/")]
    public IActionResult ApiList()
    {
        var model = new QueryRulesListViewModel(service.GetAll());

        return CreateSafeJsonResult(model);
    }

    [HttpGet]
    [Route("/stott.robotshandler/api/query-rules/[action]")]
    public IActionResult Details(string? id)
    {
        if (!Guid.TryParse(id, out var queryRuleId) || !Guid.Empty.Equals(queryRuleId))
        {
            throw new ArgumentException("Id cannot be parsed as a valid GUID.", nameof(id));
        }

        var model = service.Get(queryRuleId);

        return CreateSafeJsonResult(model);
    }

    [HttpPost]
    [Route("/stott.robotshandler/api/query-rules/[action]")]
    public IActionResult Save(SaveQueryRuleMode formSubmitModel)
    {
        try
        {
            if (service.DoesConflictExists(formSubmitModel))
            {
                return new ContentResult
                {
                    StatusCode = (int)HttpStatusCode.Conflict,
                    Content = "This specific query rule already exists.",
                    ContentType = "text/plain"
                };
            }
            
            service.Save(formSubmitModel);

            return new OkResult();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to save Query Rule for {queryName}", formSubmitModel.QueryName);
            return new ContentResult
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Content = exception.Message,
                ContentType = "text/plain"
            };
        }
    }

    [HttpDelete]
    [Route("/stott.robotshandler/api/query-rules/[action]/{id}")]
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

            service.Delete(id);

            return new OkResult();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to delete this query rule.");
            return new ContentResult
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Content = exception.Message,
                ContentType = "text/plain"
            };
        }
    }
}