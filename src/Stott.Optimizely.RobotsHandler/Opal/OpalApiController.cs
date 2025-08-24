using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Stott.Optimizely.RobotsHandler.Common;
using Stott.Optimizely.RobotsHandler.Opal.Models;
using Stott.Optimizely.RobotsHandler.Robots;

namespace Stott.Optimizely.RobotsHandler.Opal;

[ApiExplorerSettings(IgnoreApi = true)]
[AllowAnonymous]
public sealed class OpalApiController : BaseApiController
{
    private readonly IRobotsContentService _service;

    private readonly ILogger<OpalApiController> _logger;

    public OpalApiController(
        IRobotsContentService service,
        ILogger<OpalApiController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    [Route("/stott.robotshandler/opal/discovery/")]
    [OpalAuthorization(OpalAuthorizationLevel.Read)]
    public IActionResult Discovery()
    {
        var authorizationLevel = HttpContext.Items[RobotsConstants.OpalAuthorizationLevelKey] as OpalAuthorizationLevel? ?? OpalAuthorizationLevel.None;
        var model = new FunctionsRoot { Functions = new List<Function>() };

        if (authorizationLevel >= OpalAuthorizationLevel.Read)
        {
            model.Functions.Add(new Function
            {
                Name = "getrobottxtconfigurations",
                Description = "Get a collection of robot.txt configurations optionally filtered by host name.",
                Parameters = new List<FunctionParameter>
                {
                    new FunctionParameter
                    {
                        Name = "hostName",
                        Type = "string",
                        Description = "The host name to filter the robot.txt configurations by.",
                        Required = false
                    }
                },
                Endpoint = "/tools/get-robot-txt-configurations/",
                HttpMethod = "POST"
            });
        }

        if (authorizationLevel >= OpalAuthorizationLevel.Write)
        {
            model.Functions.Add(new Function
            {
                Name = "saverobottxtconfigurations",
                Description = "Saves robots content for a specific Id or Host Name. Where an update is being performed for a host name and a specific configuration does not exist, one will be created.",
                Parameters = new List<FunctionParameter>
                {
                    new FunctionParameter
                    {
                        Name = "robotsId",
                        Type = "Guid",
                        Description = "The Id of the robots.txt configuration that is to be updated.",
                        Required = false
                    },
                    new FunctionParameter
                    {
                        Name = "hostName",
                        Type = "string",
                        Description = "The host name of of the robots.txt configuration that is to be updated.",
                        Required = false
                    },
                    new FunctionParameter
                    {
                        Name = "RobotsTxtContent",
                        Type = "string",
                        Description = "The robots.txt content to be saved.  This should be a valid robots.txt content and must be provided.",
                        Required = true
                    }
                },
                Endpoint = "/tools/save-robot-txt-configurations/",
                HttpMethod = "POST"
            });
        }

        return CreateSafeJsonResult(model);
    }

    [HttpPost]
    [Route("/stott.robotshandler/opal/tools/get-robot-txt-configurations/")]
    [Route("/stott.robotshandler/opal/discovery/tools/get-robot-txt-configurations/")]
    [OpalAuthorization(OpalAuthorizationLevel.Read)]
    public IActionResult GetRobotTxtConfigurations([FromBody]ToolRequest<GetRobotTextConfigurationsQuery> model)
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

                return CreateSafeJsonResult(ToOpalModel(specificConfiguration));
            }

            return CreateSafeJsonResult(ToOpalModels(configurations));
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "An error was encountered while processing the robot-txt-configurations tool.");
            throw;
        }
    }

    [HttpPost]
    [Route("/stott.robotshandler/opal/tools/save-robot-txt-configuration/")]
    [Route("/stott.robotshandler/opal/discovery/tools/save-robot-txt-configuration/")]
    [OpalAuthorization(OpalAuthorizationLevel.Write)]
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

    private static IEnumerable<SiteRobotsOpalModel> ToOpalModels(IEnumerable<SiteRobotsViewModel> siteModels)
    {
        if (siteModels is null)
        {
            yield break;
        }

        foreach(var siteModel in siteModels)
        {
            yield return ToOpalModel(siteModel);
        }
    }

    private static SiteRobotsOpalModel ToOpalModel(SiteRobotsViewModel siteModel)
    {
        return new SiteRobotsOpalModel
        {
            Id = siteModel.Id,
            SiteName = siteModel.SiteName,
            IsDefaultForSite = siteModel.IsForWholeSite,
            SpecificHost = siteModel.SpecificHost,
            RobotsContent = siteModel.RobotsContent
        };
    }
}