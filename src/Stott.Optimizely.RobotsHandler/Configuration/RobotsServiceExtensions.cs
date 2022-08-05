using Microsoft.Extensions.DependencyInjection;

using Stott.Optimizely.RobotsHandler.Presentation;
using Stott.Optimizely.RobotsHandler.Services;

namespace Stott.Optimizely.RobotsHandler.Configuration
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddRobotsHandler(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IRobotsContentService, RobotsContentService>();
            serviceCollection.AddTransient<IRobotsContentRepository, RobotsContentRepository>();
            serviceCollection.AddTransient<IRobotsEditViewModelBuilder, RobotsEditViewModelBuilder>();
            serviceCollection.AddTransient<IRobotsListViewModelBuilder, RobotsListViewModelBuilder>();

            return serviceCollection;
        }
    }
}
