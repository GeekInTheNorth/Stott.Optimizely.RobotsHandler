using System;

namespace Stott.Optimizely.RobotsHandler.Opal;

public class TokenModel
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public string RobotsScope { get; set; }

    public string LlmsScope { get; set; }

    public string Token { get; set; }
}