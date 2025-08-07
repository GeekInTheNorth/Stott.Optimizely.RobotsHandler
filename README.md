# Stott.Optimizely.RobotsHandler

[![Platform](https://img.shields.io/badge/Platform-.NET%206-blue.svg?style=flat)](https://docs.microsoft.com/en-us/dotnet/)
[![Platform](https://img.shields.io/badge/Optimizely-%2012-blue.svg?style=flat)](http://world.episerver.com/cms/)
[![GitHub](https://img.shields.io/github/license/GeekInTheNorth/Stott.Optimizely.RobotsHandler)](https://github.com/GeekInTheNorth/Stott.Optimizely.RobotsHandler/blob/main/LICENSE.txt)
![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/GeekInTheNorth/Stott.Optimizely.RobotsHandler/dotnet.yml?branch=main)
![Nuget](https://img.shields.io/nuget/v/Stott.Optimizely.RobotsHandler)

This is an admin extension for Optimizely CMS 12+ for managing robots content. Stott Robots Handler is a free to use module, however if you want to show your support, buy me a coffee on ko-fi:

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/V7V0RX2BQ)

## Robots.Txt Management

Robots.txt content can be managed on a per site and host definition basis.  A host of "default" applies to all unspecified hosts within a site, while specific host definitions will only apply to the specific host.

![Stott Robots Handler, Robots.txt management](/StottRobotsList1.png)

## Environment Robots

_Introduced within version 4.0.0_

Environment Robots allows you to configure the meta robots tag and `X-Robots-Tag` header for all page requests within the current environment.  This functionality provides the ability to prevent search engine robots from scanning indexing a site that is a lower level environment or a production environment that is not ready for general consumption.

Options will always exist for Integration, Preproduction, Production and the current environment name. This allows you to preconfigure a lower enviroment when cloning content from production to lower environments.

![Stott Robots Handler, Environment robots configuration](/StottRobotsList2.png)

When a configuration is active, a Meta Tag Helper will look for and update the meta robots tag while a middleware will include the `X-Robots-Tag` header.  Its best in this case that your solution always renders the meta robots element and allow the Meta Tag Helper to either override it or remove it where needed.

The meta tag helper will execute for any `meta` tag with a `name` attribute.  The logic within the robots tag helper will only execute where the `name` attribute has a value of `robots`.  In such a circumstance it will perform one of the following actions

- When `name` is `robots`
  - If the page has a robots definition and an environment configuration is not set, then the page robots value will be used.
  - If the page has a robots definition and an environment configuration is set, then the environment configuration replaces the page definition.
  - If the page does not have robots definition and an environment configuration is not set, then the meta tag will be removed.
  - If the page does not have a robots definition and an environment configuration is set, then the meta tag will use the environment configuration
- When `name` is not `robots`
  - Preserve existing state.

Examples:
| Page Robots | Environment Robots | Result |
|-------------|--------------------|--------|
| noindex,nofollow | noindex,nofollow,noimageindex | noindex,nofollow,noimageindex |
| noindex,nofollow | - | noindex,nofollow |
| - | noindex,nofollow,noimageindex | noindex,nofollow,noimageindex |
| - | - | _meta robots tag is removed_ |

## LLMS.txt Management

_Introduced within version 5.0.0_

Llms.txt content is a new standard in making website context and content easy to understand to Large Language Models (AI).  This standard uses [markdown](https://www.markdownguide.org/basic-syntax/) as it is both easy to understand for both humans and AI.  You can learn more about llms.txt [here](https://llmstxt.org/).

Llms.txt content can be managed on a per site and host definition basis.  A host of "default" applies to all unspecified hosts within a site, while specific host definitions will only apply to the specific host.  If Llms.txt content has not been defined, then a request to `/llms.txt` will result in a 404 response.

![Stott Robots Handler, Llms.txt management](/StottRobotsList3.png)

## Installation

Install the `Stott.Optimizely.RobotsHandler` package into your website.

### Startup.cs

You need to ensure the following lines are added to the startup class of your solution:

```
public void ConfigureServices(IServiceCollection services)
{
    services.AddRobotsHandler();
}

public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    app.UseRobotsHandler();

    app.UseEndpoints(endpoints =>
    {
        endpoints.MapContent();
        endpoints.MapControllers();
    });
}
```

The call to `services.AddRobotsHandler()` sets up the dependency injection requirements for the RobotsHandler solution and is required to ensure the solution works as intended.  This works by following the Services Extensions pattern defined by Microsoft.

The call to `app.UseRobotsHandler()` sets up the middleware required to create the `X-Robots-Tag` header

The call to `endpoints.MapControllers();` ensures that the routing for the administration page, assets and robots.txt are correctly mapped.

### Razor Files

In the `_ViewImports.cshtml` file you will need to add the following line to include meta robots tag helper.  

```
@addTagHelper *, Stott.Optimizely.RobotsHandler
```

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
| [jhope-kc](https://github.com/jhope-kc) | 1 | - |
