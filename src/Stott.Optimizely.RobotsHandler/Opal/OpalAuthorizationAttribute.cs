using System;
using System.Linq;

using EPiServer.ServiceLocation;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Stott.Optimizely.RobotsHandler.Opal;

/// <summary>
/// Attribute to validate the Opal Bearer Token against the configured tokens in the system
/// </summary>
public sealed class OpalAuthorizationAttribute : Attribute, IActionFilter
{
    public OpalAuthorizationLevel AuthorizationLevel { get; set; } = OpalAuthorizationLevel.None;

    public OpalScopeType ScopeType { get; set; } = OpalScopeType.Robots;

    public OpalAuthorizationAttribute(OpalScopeType scopeType, OpalAuthorizationLevel authorizationLevel)
    {
        ScopeType = scopeType;
        AuthorizationLevel = authorizationLevel;
    }

    /// <summary>
    /// Validates the Opal Bearer Token against the configured tokens in the system
    /// returns a 401 response if the token is not valid or does not have the required authorization level.
    /// </summary>
    /// <param name="context"></param>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        var authorizationLevel = GetAuthorization(context.HttpContext.Request);
        if (authorizationLevel < AuthorizationLevel)
        {
            context.Result = new ContentResult
            {
                StatusCode = 401,
                Content = "You are not authorized to access this resource.",
                ContentType = "text/plain"
            };
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // No action needed after execution
    }

    /// <summary>
    /// Gets the authorization level based on a bearer token.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    private OpalAuthorizationLevel GetAuthorization(HttpRequest request)
    {
        try
        {
            if (!request.Headers.TryGetValue("Authorization", out var authorizationHeader))
            {
                return OpalAuthorizationLevel.None;
            }

            var tokenValue = authorizationHeader.ToString().Split(' ').Last();
            if (string.IsNullOrWhiteSpace(tokenValue))
            {
                return OpalAuthorizationLevel.None;
            }

            var tokenRepository = ServiceLocator.Current.GetInstance<IOpalTokenRepository>();
            var tokenConfiguration = tokenRepository.List().Where(x => x.Token == tokenValue).FirstOrDefault();
            if (tokenConfiguration is null)
            {
                return OpalAuthorizationLevel.None;
            }

            var scope = ScopeType == OpalScopeType.Robots ? tokenConfiguration.RobotsScope : tokenConfiguration.LlmsScope;
            if (Enum.TryParse<OpalAuthorizationLevel>(scope, true, out var parsedLevel))
            {
                return parsedLevel;
            }

            return OpalAuthorizationLevel.None;
        }
        catch (Exception)
        {
            return OpalAuthorizationLevel.None;
        }
    }
}