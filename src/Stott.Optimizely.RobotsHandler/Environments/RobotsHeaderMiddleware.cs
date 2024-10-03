using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using Stott.Optimizely.RobotsHandler.Extensions;

namespace Stott.Optimizely.RobotsHandler.Environments;

public sealed class RobotsHeaderMiddleware
{
    private readonly RequestDelegate _next;

    public RobotsHeaderMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(
        HttpContext context,
        IEnvironmentRobotsService service,
        ILogger<RobotsHeaderMiddleware> logger)
    {
        try
        {
            var robotsConfiguration = service.GetCurrent();
            if (robotsConfiguration is { IsEnabled: true })
            {
                context.Response.Headers.AddOrUpdateHeader("X-Robots-Tag", robotsConfiguration.ToMetaContent());
            }
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "[Robots Handler] Error encountered adding robots headers.");
        }

        await _next(context);
    }
}