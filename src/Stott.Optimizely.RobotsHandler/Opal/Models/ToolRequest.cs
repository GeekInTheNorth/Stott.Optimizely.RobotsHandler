namespace Stott.Optimizely.RobotsHandler.Opal.Models;

public class ToolRequest<TModel> where TModel : class
{
    public TModel Parameters { get; set; }
}