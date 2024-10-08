using System.Collections.Generic;

using Microsoft.AspNetCore.Mvc.Filters;

namespace OptimizelyTwelveTest.Features.Attributes;

public sealed class DisallowRobotsActonFilterAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        base.OnActionExecuting(context);

        context.HttpContext.Response.Headers.TryAdd("X-Robots-Tag", "noindex, nofollow");
    }
}
