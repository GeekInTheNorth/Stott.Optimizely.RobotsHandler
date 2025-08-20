using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Stott.Optimizely.RobotsHandler.Common;
using Stott.Optimizely.RobotsHandler.Opal.Models;
using Stott.Optimizely.RobotsHandler.Robots;

namespace Stott.Optimizely.RobotsHandler.Opal;

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

    [HttpGet("/stott.robotshandler/opal/discovery/")]
    public static IActionResult Discovery()
    {
        var model = new FunctionsRoot
        {
            Functions = new List<Function>
            {
                new Function
                {
                    Name = "robottxtconfigurations",
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
                    Endpoint = "/tools/robot-txt-configurations/",
                    HttpMethod = "POST"
                }
            }
        };

        return CreateSafeJsonResult(model);
    }

    [HttpPost("/stott.robotshandler/opal/tools/robot-txt-configurations/")]
    public IActionResult GetRobotTxtConfigurations(ToolRequest<GetRobotTextConfigurationsQuery> model)
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