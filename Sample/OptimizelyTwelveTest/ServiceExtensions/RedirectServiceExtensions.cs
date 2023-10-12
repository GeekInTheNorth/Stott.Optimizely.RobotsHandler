namespace OptimizelyTwelveTest.ServiceExtensions
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;

    using EPiServer;
    using EPiServer.Core;

    using Features.Home;
    using Features.Settings;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Rewrite;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Net.Http.Headers;

    public static class RedirectServiceExtensions
    {
        public static RewriteOptions RewriteOptions = new RewriteOptions()
            .Add(context =>
            {
                if (context.HttpContext.Request.Path.StartsWithSegments("/util") ||
                context.HttpContext.Request.Path.StartsWithSegments("/EPiServer") ||
                context.HttpContext.Request.Path.StartsWithSegments("/EPiServer.Forms") ||
                context.HttpContext.Request.Path.StartsWithSegments("/episerver") ||
                context.HttpContext.Request.Path.StartsWithSegments("/modules") ||
                context.HttpContext.Request.Path.StartsWithSegments("/Login") ||
                context.HttpContext.Request.Path.StartsWithSegments("/App_Themes") ||
                context.HttpContext.Request.Path.StartsWithSegments("/bundles") ||
                context.HttpContext.Request.Path.StartsWithSegments("/find_v2") ||
                context.HttpContext.Request.Path.StartsWithSegments("/siteassets") ||
                context.HttpContext.Request.Path.StartsWithSegments("/globalassets") ||
                context.HttpContext.Request.Path.StartsWithSegments("/contentassets") ||
                context.HttpContext.Request.Path.StartsWithSegments("/link") ||
                context.HttpContext.Request.Path.StartsWithSegments("/sitemap") ||
                context.HttpContext.Request.Path.StartsWithSegments("/sitemap.xml") ||
                // context.HttpContext.Request.Path.StartsWithSegments("/stott.robotshandler") ||
                context.HttpContext.Request.Path.StartsWithSegments("/ClientResources") ||
                context.HttpContext.Request.Path.StartsWithSegments("/Static") ||
                context.HttpContext.Request.Path.StartsWithSegments("/localobjectcache") ||
                context.HttpContext.Request.Path.StartsWithSegments("/api") ||
                context.HttpContext.Request.Path.StartsWithSegments("/WebResource") ||
                context.HttpContext.Request.Path.StartsWithSegments("/error") ||
                context.HttpContext.Request.Path.StartsWithSegments("/manifest.json") ||
                context.HttpContext.Request.Path.StartsWithSegments("/favicon.ico"))
                {
                    context.Result = RuleResult.SkipRemainingRules;
                }
            })
            // Redirect to HTTPS => issues a 301 and the browser sends in another request, OK
            .AddRedirectToHttpsPermanent()
            // Enforce trailing slash. AND lower case.
            .Add(new CustomRule());

        public static IApplicationBuilder AddRedirects(this IApplicationBuilder app)
        {
            app.UseRewriter(RewriteOptions);

            return app;
        }
    }

    public class CustomRule : IRule
    {
        public int StatusCode { get; } = (int)HttpStatusCode.MovedPermanently;

        public void ApplyRule(RewriteContext context)
        {
            
            HttpRequest request = context.HttpContext.Request;
            PathString path = context.HttpContext.Request.Path;
            HostString host = context.HttpContext.Request.Host;
            HttpResponse response = context.HttpContext.Response;

            // add trailing slash if required
            var newPath = (PathString)Regex.Replace(path, "(.*[^/])$", "$1/");

            if (!newPath.ToString().Contains("/GetaOptimizelySitemaps/", StringComparison.OrdinalIgnoreCase) && !newPath.ToString().Contains("/GetaNotFoundHandlerAdmin/", StringComparison.OrdinalIgnoreCase))
            {
                // if a slash was applied, set the flags for a redirect
                if (!path.Equals(newPath, StringComparison.Ordinal))
                {
                    response.StatusCode = StatusCode;
                    response.Headers[HeaderNames.Location] = (request.Scheme + "://" + host.Value + request.PathBase.Value + newPath) + request.QueryString;
                    context.Result = RuleResult.EndResponse;
                }

                // lowercase if required
                if (newPath.HasValue && newPath.Value.Any(char.IsUpper) || host.HasValue && host.Value.Any(char.IsUpper))
                {
                    // lowercase the url and set the header location
                    response.Headers[HeaderNames.Location] = (request.Scheme + "://" + host.Value + request.PathBase.Value + newPath).ToLower() + request.QueryString;

                    // set the status code and end the processing of the rules
                    response.StatusCode = StatusCode;
                    context.Result = RuleResult.EndResponse;
                }
            }
        }
    }
}
