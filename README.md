# Stott.Optimizely.RobotsHandler

[![Platform](https://img.shields.io/badge/Platform-.NET%206-blue.svg?style=flat)](https://docs.microsoft.com/en-us/dotnet/)
[![Platform](https://img.shields.io/badge/Optimizely-%2012-blue.svg?style=flat)](http://world.episerver.com/cms/)

This is a new admin extension for Optimizely CMS 12+ for managing robots.txt on a per site basis.  The Stott.Optimizely.RobotsHandler project is a Razor Class Library which contains Razor Files, static JS files and relevent business logic specific to this functionality.

## Configuration

After pulling in a reference to the Stott.Optimizely.RobotsHandler project, you only need to ensure the following lines are added to the startup class of your solution:

```
public void ConfigureServices(IServiceCollection services)
{
    services.AddRazorPages();
    services.AddRobotsHandler();
}
```

The call to ```services.AddRazorPages()``` is a standard .NET call to ensure razor pages are included in your solution.

The call to ```services.AddRobotsHandler()``` sets up the dependency injection requirements for the RobotsHandler solution and is required to ensure the solution works as intended.  This works by following the Services Extensions pattern defined by microsoft.

This solution also includes an implementation of ```IMenuProvider``` which ensures that the Robots Handler administration pages are included in the CMS Admin menu under the title of "Robots".  You do not have to do anything to make this work as Optimizely CMS will scan and action all implementations of ```IMenuProvider```.

### Authorisation Configuration

The configuration of the module has some scope for modification by providing configuration in the service extension methods.  The provision of ```authorizationOptions``` is optional in the following example.

Example:
```C#
services.AddRobotsHandler(authorizationOptions => 
{
    authorizationOptions.AddPolicy(RobotsConstants.AuthorizationPolicy, policy =>
    {
        policy.RequireRole("WebAdmins");
    });
});
```

If the ```authorizationOptions``` is not provided, then any of the following roles will be required be default:

- CmsAdmins
- Administrator
- WebAdmins

## Contributing

I am open to contributions to the code base.  The following rules should be followed:

1. Contributions should be made by Pull Requests.
2. All commits should have a meaningful messages.
3. All commits should have a reference to your GitHub user.
4. Ideally all new changes should include appropriate unit test coverage.

## Common Issues

### RobotsAdmin.js returns a 404 error response.

The RobotsHandler is built as a Razor Class Library, this produces a manifest that tells the application about the static assets that are included within the Razor Class Library.  The `StaticWebAssetsFileProvider` will then handle the serving of these files from the Razor Class Library.  However, the `StaticWebAssetsFileProvider` is only automatically added when the `ASPNETCORE_ENVIRONMENT` is set to `Development`.  You can read more about this here: [How to deal with the "HTTP 404 '_content/Foo/Bar.css' not found"](https://dev.to/j_sakamoto/how-to-deal-with-the-http-404-content-foo-bar-css-not-found-when-using-razor-component-package-on-asp-net-core-blazor-app-aai)

This can manually be re-introduced by updating the `Program.cs` and re-introducing the `StaticWebAssetsFileProvider` for non-development environments.  The key component in this example being `StaticWebAssetsLoader.UseStaticWebAssets(ctx.HostingEnvironment, ctx.Configuration)`:

```
public class Program
{
    public static void Main(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var isDevelopment = environment == Environments.Development;

        if (isDevelopment)
        {
            //Development configuration
        }

        CreateHostBuilder(args, isDevelopment).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args, bool isDevelopment)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureCmsDefaults()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
                if (!isDevelopment)
                {
                    webBuilder.ConfigureAppConfiguration((ctx, cb) =>
                    {
                        StaticWebAssetsLoader.UseStaticWebAssets(ctx.HostingEnvironment, ctx.Configuration);
                    });
                }
            });
    }
}
```

            
