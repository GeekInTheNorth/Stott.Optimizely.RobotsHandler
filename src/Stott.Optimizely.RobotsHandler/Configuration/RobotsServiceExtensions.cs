namespace Stott.Optimizely.RobotsHandler.Configuration;

using System;
using System.Collections.Generic;

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

using Stott.Optimizely.RobotsHandler.Common;
using Stott.Optimizely.RobotsHandler.Presentation;
using Stott.Optimizely.RobotsHandler.Services;

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
