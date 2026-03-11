namespace OptimizelyTwelveTest;

using System;
using EPiServer.Cms.Shell.UI;
using EPiServer.Cms.UI.AspNetIdentity;
using EPiServer.Data;
using EPiServer.DependencyInjection;
using EPiServer.Scheduler;
using EPiServer.Web.Routing;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using Optimizely.Graph.DependencyInjection;
using OptimizelyTwelveTest.Features.Common;

using ServiceExtensions;

using Stott.Optimizely.RobotsHandler.Common;
using Stott.Optimizely.RobotsHandler.Configuration;

public class Startup(IWebHostEnvironment webHostingEnvironment)
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddCms()
                .AddCmsAspNetIdentity<ApplicationUser>()
                .AddAdminUserRegistration(options => { options.Behavior = RegisterAdminUserBehaviors.Enabled; })
                .AddVisitorGroups()
                .AddContentGraph()
                .AddContentManager()
                .AddCmsTagHelpers()
                .AddCustomDependencies();

        services.Configure<DataAccessOptions>(options =>
        {
            options.UpdateDatabaseCompatibilityLevel = true;
        });

        if (webHostingEnvironment.IsDevelopment())
        {
            services.Configure<SchedulerOptions>(o =>
            {
                o.Enabled = false;
            });
        }

        services.AddRazorPages();
        
        // Various serialization formats.
        //// services.AddMvc().AddNewtonsoftJson();
        services.AddMvc().AddJsonOptions(config =>
        {
            config.JsonSerializerOptions.PropertyNamingPolicy = new UpperCaseNamingPolicy();
        });

        //// services.AddRobotsHandler();
        services.AddRobotsHandler(authorizationOptions =>
        {
            authorizationOptions.AddPolicy(RobotsConstants.AuthorizationPolicy, policy =>
            {
                policy.RequireRole("RobotAdmins", "WebAdmins", "Everyone");
            });
        });

        //services.AddStottSecurity(cspSetupOptions =>
        //{
        //    cspSetupOptions.ConnectionStringName = "EPiServerDB";
        //},
        //authorizationOptions =>
        //{
        //    authorizationOptions.AddPolicy(CspConstants.AuthorizationPolicy, policy =>
        //    {
        //        policy.RequireRole("WebAdmins");
        //    });
        //});

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
            if (context.Request is not null && !context.Request.Path.Value.StartsWith("/optimizely/", StringComparison.OrdinalIgnoreCase))
            {
                if (context.Response.Headers.ContainsKey(HeaderNames.CacheControl))
                {
                    context.Response.Headers[HeaderNames.CacheControl] = "no-cache, max-age=0";
                }
                else
                {
                    context.Response.Headers.Append(HeaderNames.CacheControl, "no-cache, max-age=0");
                }
            }

            await next();
        });

        app.AddRedirects();
        app.UseStaticFiles();
        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();
        // app.UseStottSecurity();
        app.UseRobotsHandler();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapContent();
            endpoints.MapControllers();
        });
    }
}