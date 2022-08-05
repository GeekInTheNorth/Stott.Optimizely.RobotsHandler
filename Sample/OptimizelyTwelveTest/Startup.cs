﻿namespace OptimizelyTwelveTest
{
    using EPiServer.Cms.UI.AspNetIdentity;
    using EPiServer.Scheduler;
    using EPiServer.Web.Routing;

    using MediatR;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    using OptimizelyTwelveTest.Features.Common;

    using ServiceExtensions;

    using Stott.Optimizely.RobotsHandler.Configuration;

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
                    .AddMediatR(typeof(GroupNames).Assembly)
                    .AddRobotsHandler()
                    .AddCustomDependencies();

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

            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapContent();
                endpoints.MapRazorPages();
            });
        }
    }
}
