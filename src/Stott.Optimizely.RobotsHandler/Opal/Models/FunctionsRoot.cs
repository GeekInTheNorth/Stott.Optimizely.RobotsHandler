using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Stott.Optimizely.RobotsHandler.Opal.Models;

public class FunctionsRoot
{
    [JsonPropertyName("functions")]
    public List<Function> Functions { get; set; }
}