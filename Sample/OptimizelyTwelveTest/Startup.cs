namespace OptimizelyTwelveTest;

using System;

using EPiServer.Cms.UI.AspNetIdentity;
using EPiServer.Scheduler;
using EPiServer.Web.Routing;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;

using OptimizelyTwelveTest.Features.Common;
using OptimizelyTwelveTest.Features.Search;

using ServiceExtensions;

using Stott.Optimizely.RobotsHandler.Common;
using Stott.Optimizely.RobotsHandler.Configuration;
using Stott.Optimizely.RobotsHandler.Presentation;
using Stott.Security.Optimizely.Common;
using Stott.Security.Optimizely.Features.Configuration;

public class Startup
{
    private readonly IWebHostEnvironment _webHostingEnvironment;

    public Startup(IWebHostEnvironment webHostingEnvironment)
    {
        _webHostingEnvironment = webHostingEnvironment;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        if (_webHostingEnvironment.IsDevelopment())
        {
            services.Configure<SchedulerOptions>(o =>
            {
                o.Enabled = false;
            });
        }

        services.AddRazorPages();
        services.AddCmsAspNetIdentity<ApplicationUser>();
        
        // Various serialization formats.
        //// services.AddMvc().AddNewtonsoftJson();
        services.AddMvc().AddJsonOptions(config =>
        {
            config.JsonSerializerOptions.PropertyNamingPolicy = new UpperCaseNamingPolicy();
        });

        services.AddCms()
                .AddFind()
                .AddMediatR(config =>
                {
                    config.RegisterServicesFromAssemblyContaining<RobotsApiController>();
                    config.RegisterServicesFromAssemblyContaining<SearchPageController>();
                })
                .AddCustomDependencies()
                .AddSwaggerGen();

        //// services.AddRobotsHandler();
        services.AddRobotsHandler(authorizationOptions =>
        {
            authorizationOptions.AddPolicy(RobotsConstants.AuthorizationPolicy, policy =>
            {
                policy.RequireRole("RobotAdmins");
            });
        });

        services.AddStottSecurity(cspSetupOptions =>
        {
            cspSetupOptions.ConnectionStringName = "EPiServerDB";
        },
        authorizationOptions =>
        {
            authorizationOptions.AddPolicy(CspConstants.AuthorizationPolicy, policy =>
            {
                policy.RequireRole("WebAdmins");
            });
        });

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/util/Login";
        });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseResponseCaching();
        app.Use(async (context, next) =>
        {
            if (context.Request is not null && !context.Request.Path.Value.StartsWith("/episerver/", StringComparison.OrdinalIgnoreCase))
            {
                if (context.Response.Headers.ContainsKey(HeaderNames.CacheControl))
                {
                    context.Response.Headers[HeaderNames.CacheControl] = "no-cache, max-age=0";
                }
                else
                {
                    context.Response.Headers.Add(HeaderNames.CacheControl, "no-cache, max-age=0");
                }
            }

            await next();
        });

        app.AddRedirects();
        app.UseStaticFiles();
        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseStottSecurity();
        app.UseRobotsHandler();

        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapContent();
            endpoints.MapRazorPages();
        });
    }
}