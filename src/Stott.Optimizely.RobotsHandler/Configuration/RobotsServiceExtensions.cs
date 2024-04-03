namespace Stott.Optimizely.RobotsHandler.Configuration;

using System;
using System.Collections.Generic;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using Stott.Optimizely.RobotsHandler.Common;
using Stott.Optimizely.RobotsHandler.Presentation;
using Stott.Optimizely.RobotsHandler.Services;
using Stott.Security.Optimizely.Features.StaticFile;

public static class ServiceExtensions
{
    public static IServiceCollection AddRobotsHandler(
        this IServiceCollection serviceCollection,
        Action<AuthorizationOptions> authorizationOptions = null)
    {
        serviceCollection.AddTransient<IRobotsContentService, RobotsContentService>();
        serviceCollection.AddTransient<IRobotsContentRepository, RobotsContentRepository>();
        serviceCollection.AddTransient<IRobotsEditViewModelBuilder, RobotsEditViewModelBuilder>();
        serviceCollection.AddTransient<IRobotsListViewModelBuilder, RobotsListViewModelBuilder>();
        serviceCollection.AddScoped<IStaticFileResolver, StaticFileResolver>();

        // Authorization
        if (authorizationOptions != null)
        {
            serviceCollection.AddAuthorization(authorizationOptions);
        }
        else
        {
            var allowedRoles = new List<string> { "CmsAdmins", "Administrator", "WebAdmins" };
            serviceCollection.AddAuthorization(authorizationOptions =>
            {
                authorizationOptions.AddPolicy(RobotsConstants.AuthorizationPolicy, policy =>
                {
                    policy.RequireRole(allowedRoles);
                });
            });
        }

        return serviceCollection;
    }

    public static IApplicationBuilder AddRobotsHandler(this IApplicationBuilder builder)
    {
        return builder.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                name: "RobotsLandingPage",
                pattern: "{controller=RobotsLandingPage}/{action=Index}/{id?}"
                );
            endpoints.MapControllerRoute("RobotsApi", "api/{controller}/{action}/{id?}");
        });
    }
}
