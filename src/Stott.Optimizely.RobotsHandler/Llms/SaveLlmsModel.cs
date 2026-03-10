using System;

namespace Stott.Optimizely.RobotsHandler.Llms;

public sealed class SaveLlmsModel
{
    public Guid Id { get; set; }

    public string? AppId { get; set; }

    public string? AppName { get; set; }

    public string? SpecificHost { get; set; }

    public string? LlmsContent { get; set; }
}
