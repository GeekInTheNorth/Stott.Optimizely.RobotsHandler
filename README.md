# Stott.Optimizely.RobotsHandler

[![Platform](https://img.shields.io/badge/Platform-.NET%206-blue.svg?style=flat)](https://docs.microsoft.com/en-us/dotnet/)
[![Platform](https://img.shields.io/badge/Optimizely-%2012-blue.svg?style=flat)](http://world.episerver.com/cms/)
[![GitHub](https://img.shields.io/github/license/GeekInTheNorth/Stott.Optimizely.RobotsHandler)](https://github.com/GeekInTheNorth/Stott.Optimizely.RobotsHandler/blob/main/LICENSE.txt)
![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/GeekInTheNorth/Stott.Optimizely.RobotsHandler/dotnet.yml?branch=develop)
![Nuget](https://img.shields.io/nuget/v/Stott.Optimizely.RobotsHandler)

This is an admin extension for Optimizely CMS 12+ for managing robots.txt on a per site and host definition basis.  The Stott.Optimizely.RobotsHandler project is a Razor Class Library which contains Razor Files, static JS files and relevent business logic specific to this functionality.

![Stott Robots Handler](/StottRobotsList1.png)

Stott Robots Handler is a free to use module, however if you want to show your support, buy me a coffee on ko-fi:

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/V7V0RX2BQ)

## Configuration

### Startup.cs

After pulling in a reference to the Stott.Optimizely.RobotsHandler project, you need to ensure the following lines are added to the startup class of your solution:

```
public void ConfigureServices(IServiceCollection services)
{
    services.AddRobotsHandler();
}

public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapContent();
        endpoints.MapControllers();
    });
}
```

The call to ```services.AddRobotsHandler()``` sets up the dependency injection requirements for the RobotsHandler solution and is required to ensure the solution works as intended.  This works by following the Services Extensions pattern defined by Microsoft.

The call to ```endpoints.MapControllers();``` ensures that the routing for the administration page, assets and robots.txt are correctly mapped.

### Program.cs

As this package includes static files such as JS and CSS files within the Razor Class Library, your solution must be configured to use Static Web Assets.  This is done by adding `webBuilder.UseStaticWebAssets();` to your `Program.cs` as follows:

```
Host.CreateDefaultBuilder(args)
    .ConfigureCmsDefaults()
    .ConfigureWebHostDefaults(webBuilder =>
    {
        webBuilder.UseStartup<Startup>();
        webBuilder.UseStaticWebAssets();
    });
```

You can read more about shared assets in Razor Class Libraries here: [Create reusable UI using the Razor class library project in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/razor-pages/ui-class?view=aspnetcore-6.0&tabs=visual-studio)

### Adding Robots Admin to the menus.

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

If the ```authorizationOptions``` is not provided, then any of the following roles will be required by default:

- CmsAdmins
- Administrator
- WebAdmins

### Authentication With Optimizely Opti ID

If you are using the new Optimizely Opti ID package for authentication into Optimizely CMS and the rest of the Optimizely One suite, then you will need to define the `authorizationOptions` for this module as part of your application start up. This should be a simple case of adding `policy.AddAuthenticationSchemes(OptimizelyIdentityDefaults.SchemeName);` to the `authorizationOptions` as per the example below.

```C#
services.AddRobotsHandler(authorizationOptions =>
{
    authorizationOptions.AddPolicy(RobotsConstants.AuthorizationPolicy, policy =>
    {
        policy.AddAuthenticationSchemes(OptimizelyIdentityDefaults.SchemeName);
        policy.RequireRole("WebAdmins");
    });
});
```

## Contributing

I am open to contributions to the code base.  The following rules should be followed:

1. Contributions should be made by Pull Requests.
2. All commits should have a meaningful message.
3. All commits should have a reference to your GitHub user.
4. Ideally, all new changes should include appropriate unit test coverage.

### Contributors

Thank you for your feedback and contributions go to the following members of the community:

| Contributor | Bug Reports | Pull Requests |
|-------------|-------------|---------------|
| [Paul Mcgann](https://github.com/paulmcgann) | 1 | 1 |
| [Ellinge](https://github.com/ellinge) | 1 | 1 |
| [Tomas Hensrud Gulla](https://github.com/tomahg) | - | 1 |
| [Anish Peethambaran](https://github.com/Anish-Peethambaran) | 1 | - |
| [Deepa V Puranik](https://github.com/Deepa-Puranik) | 1 | - |
| [Mahdi Shahbazi](https://github.com/mahdishahbazi) | 1 | - |
| [Praveen Soni](https://world.optimizely.com/System/Users-and-profiles/Community-Profile-Card/?userId=fd64fb7a-ba91-e911-a968-000d3a441525) | 1 | - |
