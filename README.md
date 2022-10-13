# Stott.Optimizely.RobotsHandler

[![Platform](https://img.shields.io/badge/Platform-.NET%206-blue.svg?style=flat)](https://docs.microsoft.com/en-us/dotnet/)
[![Platform](https://img.shields.io/badge/Optimizely-%2012-blue.svg?style=flat)](http://world.episerver.com/cms/)

This is a new admin extension for Optimizely CMS 12+ for managing robots.txt on a per site basis.  The Stott.Optimizely.RobotsHandler project is a Razor Class Library which contains Razor Files, static JS files and relevent business logic specific to this functionality.

## Configuration

### Startup.cs

After pulling in a reference to the Stott.Optimizely.RobotsHandler project, you need to ensure the following lines are added to the startup class of your solution:

```
public void ConfigureServices(IServiceCollection services)
{
    services.AddRazorPages();
    services.AddRobotsHandler();
}
```

The call to ```services.AddRazorPages()``` is a standard .NET call to ensure razor pages are included in your solution.

The call to ```services.AddRobotsHandler()``` sets up the dependency injection requirements for the RobotsHandler solution and is required to ensure the solution works as intended.  This works by following the Services Extensions pattern defined by microsoft.

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

The RobotsHandler is built as a Razor Class Library, this produces a manifest that tells the application about the static assets that are included within the Razor Class Library.  This is solved by adding `webBuilder.UseStaticWebAssets();` to the `ConfigureWebHostDefaults` method in `Program.cs`.  Please see the configuration section above.

### Projects that do not use Razor MVC lead to missing assets

Stott.Optimizely.RobotsHandler has been built as a Razor Class Library, this is predicated on a build being compatible with Razor MVC.  If your build does not use Razor MVC and the build pipeline does not inclue the output of such, then this can cause the admin interface not to work.  In this scenario this will require you to update your build pipeline to include these assets.

The following is a YAML example cloned from a screenshot where this problem was resolved:
```
- task: CopyFiles@2
  inputs:
    SourceFolder: '$(projectName)/obj/$(BuildConfiguration)/net6.0/PubTmp/Out/wwwrooot'
    Contents: '**'
    CleanTargetFolder: false
    TargetFolder: '$(Agent.TempDirectory)/$(today)/wwwroot/wwwroot/'
```

A big thank you goes to [Praveen Soni](https://world.optimizely.com/System/Users-and-profiles/Community-Profile-Card/?userId=fd64fb7a-ba91-e911-a968-000d3a441525) who helped identify this as an issue.
