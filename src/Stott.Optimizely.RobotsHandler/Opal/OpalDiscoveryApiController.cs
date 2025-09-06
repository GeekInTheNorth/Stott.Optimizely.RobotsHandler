using System.Collections.Generic;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Stott.Optimizely.RobotsHandler.Common;
using Stott.Optimizely.RobotsHandler.Opal.Models;

namespace Stott.Optimizely.RobotsHandler.Opal;

[ApiExplorerSettings(IgnoreApi = true)]
[AllowAnonymous]
public sealed class OpalDiscoveryApiController : BaseApiController
{
    [HttpGet]
    [Route("/stott.robotshandler/opal/discovery/")]
    public IActionResult Discovery()
    {
        var model = new FunctionsRoot
        {
            Functions = new List<Function>
            {
                new Function
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
                },
                new Function
                {
                    Name = "saverobottxtconfigurations",
                    Description = "Saves robots.txt content for a specific Id or Host Name. Where an update is being performed for a host name and a specific configuration does not exist, one will be created.",
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
                    Endpoint = "/tools/save-robot-txt-configuration/",
                    HttpMethod = "POST"
                },
                new Function
                {
                    Name = "getllmstxtconfigurations",
                    Description = "Get a collection of llms.txt configurations optionally filtered by host name.",
                    Parameters = new List<FunctionParameter>
                    {
                        new FunctionParameter
                        {
                            Name = "hostName",
                            Type = "string",
                            Description = "The host name to filter the llms.txt configurations by.",
                            Required = false
                        }
                    },
                    Endpoint = "/tools/get-llms-txt-configurations/",
                    HttpMethod = "POST"
                },
                new Function
                {
                    Name = "savellmstxtconfigurations",
                    Description = "Saves llms.txt content for a specific Id or Host Name. Where an update is being performed for a host name and a specific configuration does not exist, one will be created.",
                    Parameters = new List<FunctionParameter>
                    {
                        new FunctionParameter
                        {
                            Name = "llmsId",
                            Type = "Guid",
                            Description = "The Id of the llms.txt configuration that is to be updated.",
                            Required = false
                        },
                        new FunctionParameter
                        {
                            Name = "hostName",
                            Type = "string",
                            Description = "The host name of of the llms.txt configuration that is to be updated.",
                            Required = false
                        },
                        new FunctionParameter
                        {
                            Name = "LlmsTxtContent",
                            Type = "string",
                            Description = "The llms.txt content to be saved.  This should be a valid llms.txt content and must be provided.",
                            Required = true
                        }
                    },
                    Endpoint = "/tools/save-llms-txt-configuration/",
                    HttpMethod = "POST"
                }
            }
        };

        return CreateSafeJsonResult(model);
    }
}