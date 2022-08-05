# Stott.Optimizely.RobotsHandler

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

## Contributing

I am open to contributions to the code base.  The following rules should be followed:

1. Contributions should be made by Pull Requests.
2. All commits should have a meaningful messages.
3. All commits should have a reference to your GitHub user.
4. Ideally all new changes should include appropriate unit test coverage.
