using System;

namespace Stott.Optimizely.RobotsHandler.Robots;

public sealed class SaveRobotsModel
{
    public Guid Id { get; set; }

    public string? AppId { get; set; }

    public string? AppName { get; set; }

    public string? SpecificHost { get; set; }

    public string? RobotsContent { get; set; }
}