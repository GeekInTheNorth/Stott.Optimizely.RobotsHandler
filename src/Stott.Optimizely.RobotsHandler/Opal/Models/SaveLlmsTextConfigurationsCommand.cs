namespace Stott.Optimizely.RobotsHandler.Opal.Models;

public class SaveLlmsTextConfigurationsCommand
{
    public string LlmsId { get; set; }

    public string HostName { get; set; }

    public string LlmsTxtContent { get; set; }
}