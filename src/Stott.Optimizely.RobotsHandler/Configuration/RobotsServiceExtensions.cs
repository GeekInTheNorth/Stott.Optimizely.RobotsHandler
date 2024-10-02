namespace Stott.Optimizely.RobotsHandler.Configuration;

using System;
using System.Collections.Generic;

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

using Stott.Optimizely.RobotsHandler.Common;
using Stott.Optimizely.RobotsHandler.Environments;
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
        serviceCollection.AddTransient<IEnvironmentRobotsService, EnvironmentRobotsService>();
        serviceCollection.AddTransient<IEnvironmentRobotsRepository, EnvironmentRobotsRepository>();
        serviceCollection.AddScoped(provider => new Lazy<IEnvironmentRobotsRepository>(() => provider.GetRequiredService<IEnvironmentRobotsRepository>()));
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
}
