using System;
using System.Linq;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Stott.Optimizely.RobotsHandler.Opal.Models;
using Stott.Optimizely.RobotsHandler.Robots;

namespace Stott.Optimizely.RobotsHandler.Opal;

[AllowAnonymous]
public sealed class OpalRobotsApiController : OpalBaseApiController
{
    private readonly IRobotsContentService _service;

    private readonly ILogger<OpalRobotsApiController> _logger;

    public OpalRobotsApiController(
        IRobotsContentService service,
        ILogger<OpalRobotsApiController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpPost]
    [Route("/stott.robotshandler/opal/tools/get-robot-txt-configurations/")]
    [Route("/stott.robotshandler/opal/discovery/tools/get-robot-txt-configurations/")]
    [OpalAuthorization(OpalScopeType.Robots, OpalAuthorizationLevel.Read)]
    public IActionResult GetRobotTxtConfigurations([FromBody] ToolRequest<GetConfigurationsQuery> model)
    {
        try
        {
            var configurations = _service.GetAll();
            if (!string.IsNullOrWhiteSpace(model?.Parameters?.HostName))
            {
                var hostName = model.Parameters.HostName.Trim();
                var specificConfiguration =
                    configurations.FirstOrDefault(x => string.Equals(x.SpecificHost, hostName, StringComparison.OrdinalIgnoreCase)) ??
                    configurations.FirstOrDefault(x => x.AvailableHosts.Any(h => string.Equals(h.HostName, hostName, StringComparison.OrdinalIgnoreCase)));

                if (specificConfiguration is null)
                {
                    return Json(new
                    {
                        Success = false,
                        Message = $"Could not locate a robots.txt config that matched the host name of {model.Parameters.HostName}."
                    });
                }

                return CreateSafeJsonResult(ConvertToModel(specificConfiguration, hostName, x => x.RobotsContent));
            }

            return CreateSafeJsonResult(ConvertToModels(configurations, x => x.RobotsContent));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error was encountered while processing the robot-txt-configurations tool.");
            throw;
        }
    }

    [HttpPost]
    [Route("/stott.robotshandler/opal/tools/save-robot-txt-configuration/")]
    [Route("/stott.robotshandler/opal/discovery/tools/save-robot-txt-configuration/")]
    [OpalAuthorization(OpalScopeType.Robots, OpalAuthorizationLevel.Write)]
    public IActionResult SaveRobotTxtConfigurations([FromBody] ToolRequest<SaveRobotTextConfigurationsCommand> model)
    {
        try
        {
            var configurations = _service.GetAll();
            var hostName = model.Parameters?.HostName?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(model.Parameters?.RobotsTxtContent))
            {
                return Json(new
                {
                    Success = false,
                    Message = $"Robots.txt content was not provided."
                });
            }

            if (Guid.TryParse(model.Parameters?.RobotsId, out var robotsId) && !Guid.Empty.Equals(robotsId))
            {
                var specificRobotsConfig = configurations.FirstOrDefault(x => Equals(x.Id, robotsId));
                if (specificRobotsConfig is null)
                {
                    return Json(new
                    {
                        Success = false,
                        Message = $"Could not locate a robots.txt config that matched the Id of {robotsId}."
                    });
                }

                var saveModel = new SaveRobotsModel
                {
                    Id = specificRobotsConfig.Id,
                    SiteId = specificRobotsConfig.SiteId,
                    SiteName = specificRobotsConfig.SiteName,
                    SpecificHost = specificRobotsConfig.SpecificHost,
                    RobotsContent = model.Parameters?.RobotsTxtContent ?? specificRobotsConfig.RobotsContent
                };

                _service.Save(saveModel);
                return Json(new
                {
                    Success = true,
                    Message = "Robots content was saved.",
                    Data = saveModel
                });
            }

            if (!string.IsNullOrWhiteSpace(hostName))
            {
                var specificConfiguration =
                    configurations.FirstOrDefault(x => string.Equals(x.SpecificHost, hostName, StringComparison.OrdinalIgnoreCase)) ??
                    configurations.FirstOrDefault(x => x.AvailableHosts.Any(h => string.Equals(h.HostName, hostName, StringComparison.OrdinalIgnoreCase)));

                var isSpecificHost = !specificConfiguration.IsForWholeSite && string.Equals(hostName, specificConfiguration.SpecificHost, StringComparison.OrdinalIgnoreCase);

                var saveModel = new SaveRobotsModel
                {
                    Id = isSpecificHost ? specificConfiguration.Id : Guid.Empty,
                    SiteId = specificConfiguration.SiteId,
                    SiteName = specificConfiguration.SiteName,
                    SpecificHost = hostName,
                    RobotsContent = model.Parameters?.RobotsTxtContent
                };

                _service.Save(saveModel);
                return Json(new
                {
                    Success = true,
                    Message = isSpecificHost ? "Robots content was updated for the specific host name." : "Robots content was created for the specific host name.",
                    Data = saveModel
                });
            }

            return Json(new
            {
                Success = false,
                Message = "Could not perform this update to a robots.txt configuration"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error was encountered while processing the robot-txt-configurations tool.");
            throw;
        }
    }
}