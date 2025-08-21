using System;

namespace Stott.Optimizely.RobotsHandler.Opal;

public class TokenModel
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public string Scope { get; set; }

    public string Token { get; set; }
}