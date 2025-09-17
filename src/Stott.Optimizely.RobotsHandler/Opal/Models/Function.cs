using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Stott.Optimizely.RobotsHandler.Opal.Models;

public class Function
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("parameters")]
    public List<FunctionParameter> Parameters { get; set; }

    [JsonPropertyName("endpoint")]
    public string Endpoint { get; set; }

    [JsonPropertyName("http_method")]
    public string HttpMethod { get; set; }
}