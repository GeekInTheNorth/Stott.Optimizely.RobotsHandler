using System.Collections.Generic;

namespace Stott.Optimizely.RobotsHandler.Opal.Models;

public class Function
{
    public string Name { get; set; }

    public string Description { get; set; }

    public List<FunctionParameter> Parameters { get; set; }

    public string Endpoint { get; set; }

    public string HttpMethod { get; set; }
}