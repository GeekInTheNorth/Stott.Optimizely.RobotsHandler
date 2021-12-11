using System;

using Stott.Optimizely.RobotsHandler.Presentation.ViewModels;

namespace Stott.Optimizely.RobotsHandler.Presentation
{
    public interface IRobotsEditViewModelBuilder
    {
        IRobotsEditViewModelBuilder WithSiteId(Guid siteId);

        RobotsEditViewModel Build();
    }
}
