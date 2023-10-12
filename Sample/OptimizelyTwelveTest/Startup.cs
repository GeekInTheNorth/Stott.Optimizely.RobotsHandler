namespace OptimizelyTwelveTest
{
    using EPiServer.Cms.UI.AspNetIdentity;
    using EPiServer.Scheduler;
    using EPiServer.Web.Routing;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    using OptimizelyTwelveTest.Features.Common;
    using Microsoft.AspNetCore.Rewrite;

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
                        config.RegisterServicesFromAssemblyContaining<RobotsController>();
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

            services.AddCspManager(cspSetupOptions =>
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

            app.AddRedirects();
            app.UseStaticFiles();
            app.UseRouting();
            

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseCspManager();

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapContent();
                endpoints.MapRazorPages();
            });
        }
    }
}