﻿namespace Stott.Optimizely.RobotsHandler.Configuration;

using System;
using System.Collections.Generic;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using Stott.Optimizely.RobotsHandler.Cache;
using Stott.Optimizely.RobotsHandler.Common;
using Stott.Optimizely.RobotsHandler.Environments;
using Stott.Optimizely.RobotsHandler.Services;
using Stott.Security.Optimizely.Features.StaticFile;

public static class ServiceExtensions
{
    /// <summary>
    /// Adds dependencies and configures the Stott Robots Handler.
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="authorizationOptions"></param>
    /// <returns></returns>
    public static IServiceCollection AddRobotsHandler(
        this IServiceCollection serviceCollection,
        Action<AuthorizationOptions> authorizationOptions = null)
    {
        serviceCollection.AddScoped<IRobotsCacheHandler, RobotsCacheHandler>();
        serviceCollection.AddScoped<IRobotsContentService, RobotsContentService>();
        serviceCollection.AddScoped<IRobotsContentRepository, RobotsContentRepository>();
        serviceCollection.AddScoped<IEnvironmentRobotsService, EnvironmentRobotsService>();
        serviceCollection.AddScoped<IEnvironmentRobotsRepository, EnvironmentRobotsRepository>();
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

    /// <summary>
    /// Sets up middleware required to handle environment specific robots headers.
    /// </summary>
    /// <param name="builder"></param>
    public static void UseRobotsHandler(this IApplicationBuilder builder)
    {
        builder.UseMiddleware<RobotsHeaderMiddleware>();
    }
}
