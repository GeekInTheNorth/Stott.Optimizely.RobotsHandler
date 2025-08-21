using System.Text.Json.Serialization;

namespace Stott.Optimizely.RobotsHandler.Opal.Models;

public class FunctionParameter
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("required")]
    public bool Required { get; set; }
}