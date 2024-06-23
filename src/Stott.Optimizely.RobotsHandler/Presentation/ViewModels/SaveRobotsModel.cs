using System;

namespace Stott.Optimizely.RobotsHandler.Presentation.ViewModels;

public sealed class SaveRobotsModel
{
    public Guid Id { get; set; }

    public Guid SiteId { get; set; }

    public string SiteName { get; set; }

    public string SpecificHost { get; set; }

    public string RobotsContent { get; set; }
}