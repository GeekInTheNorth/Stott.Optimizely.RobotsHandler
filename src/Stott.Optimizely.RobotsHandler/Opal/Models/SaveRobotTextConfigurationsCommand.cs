namespace Stott.Optimizely.RobotsHandler.Opal.Models;

public class SaveRobotTextConfigurationsCommand
{
    public string RobotsId { get; set; }

    public string HostName { get; set; }

    public string RobotsTxtContent { get; set; }
}