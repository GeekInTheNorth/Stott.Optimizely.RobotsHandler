using System;
using System.Linq;

using EPiServer.Applications;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Stott.Optimizely.RobotsHandler.Extensions;
using Stott.Optimizely.RobotsHandler.Llms;
using Stott.Optimizely.RobotsHandler.Opal.Models;

namespace Stott.Optimizely.RobotsHandler.Opal;

[AllowAnonymous]
public sealed class OpalLlmsApiController(
    ILlmsContentService service,
    IApplicationResolver applicationResolver,
    ILogger<OpalLlmsApiController> logger) : OpalBaseApiController(applicationResolver)
{
    [HttpPost]
    [Route("/stott.robotshandler/opal/tools/get-llms-txt-configurations/")]
    [Route("/stott.robotshandler/opal/discovery/tools/get-llms-txt-configurations/")]
    [OpalAuthorization(OpalScopeType.Llms, OpalAuthorizationLevel.Read)]
    public IActionResult GetLlmsTxtConfigurations([FromBody] ToolRequest<GetConfigurationsQuery> model)
    {
        try
        {
            var configurations = service.GetAll();
            var hostName = model?.Parameters?.HostName.GetSanitizedHostDomain();

            if (!string.IsNullOrWhiteSpace(hostName))
            {
                var specificConfiguration =
                    configurations.FirstOrDefault(x => string.Equals(x.SpecificHost, hostName, StringComparison.OrdinalIgnoreCase)) ??
                    configurations.FirstOrDefault(x => x.AvailableHosts.Any(h => string.Equals(h.HostName, hostName, StringComparison.OrdinalIgnoreCase)));

                if (specificConfiguration is null)
                {
                    return Json(new
                    {
                        Success = false,
                        Message = $"Could not locate a llms.txt config that matched the host name of {hostName}."
                    });
                }

                return CreateSafeJsonResult(ConvertToModel(specificConfiguration, hostName, x => x.LlmsContent));
            }

            return CreateSafeJsonResult(ConvertToModels(configurations, x => x.LlmsContent));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error was encountered while processing the llms-txt-configurations tool.");
            throw;
        }
    }

    [HttpPost]
    [Route("/stott.robotshandler/opal/tools/save-llms-txt-configuration/")]
    [Route("/stott.robotshandler/opal/discovery/tools/save-llms-txt-configuration/")]
    [OpalAuthorization(OpalScopeType.Llms, OpalAuthorizationLevel.Write)]
    public IActionResult SaveLlmsTxtConfigurations([FromBody] ToolRequest<SaveLlmsTextConfigurationsCommand> model)
    {
        try
        {
            var configurations = service.GetAll();
            var hostName = model?.Parameters?.HostName.GetSanitizedHostDomain();

            if (string.IsNullOrWhiteSpace(model?.Parameters?.LlmsTxtContent))
            {
                return Json(new
                {
                    Success = false,
                    Message = $"Llms.txt content was not provided."
                });
            }

            if (Guid.TryParse(model?.Parameters?.LlmsId, out var llmsId) && !Guid.Empty.Equals(llmsId))
            {
                var specificRobotsConfig = configurations.FirstOrDefault(x => Equals(x.Id, llmsId));
                if (specificRobotsConfig is null)
                {
                    return Json(new
                    {
                        Success = false,
                        Message = $"Could not locate a llms.txt config that matched the Id of {llmsId}."
                    });
                }

                var saveModel = new SaveLlmsModel
                {
                    Id = specificRobotsConfig.Id,
                    AppId = specificRobotsConfig.AppId,
                    AppName = specificRobotsConfig.AppName,
                    SpecificHost = specificRobotsConfig.SpecificHost,
                    LlmsContent = model.Parameters?.LlmsTxtContent ?? specificRobotsConfig.LlmsContent
                };

                service.Save(saveModel);
                return Json(new
                {
                    Success = true,
                    Message = "Llms.txt content was saved.",
                    Data = saveModel
                });
            }

            if (!string.IsNullOrWhiteSpace(hostName))
            {
                var specificConfiguration =
                    configurations.FirstOrDefault(x => string.Equals(x.SpecificHost, hostName, StringComparison.OrdinalIgnoreCase)) ??
                    configurations.FirstOrDefault(x => x.AvailableHosts.Any(h => string.Equals(h.HostName, hostName, StringComparison.OrdinalIgnoreCase))) ??
                    GetEmptySiteModel<ApplicationLlmsViewModel>(hostName);

                if (specificConfiguration is null)
                {
                    return Json(new
                    {
                        Success = false,
                        Message = $"Could not locate a site that matched the host name of {hostName}."
                    });
                }

                var isSpecificHost = !specificConfiguration.IsForWholeSite && string.Equals(hostName, specificConfiguration.SpecificHost, StringComparison.OrdinalIgnoreCase);
                var saveModel = new SaveLlmsModel
                {
                    Id = isSpecificHost ? specificConfiguration.Id : Guid.Empty,
                    AppId = specificConfiguration.AppId,
                    AppName = specificConfiguration.AppName,
                    SpecificHost = hostName,
                    LlmsContent = model?.Parameters?.LlmsTxtContent
                };

                service.Save(saveModel);
                return Json(new
                {
                    Success = true,
                    Message = isSpecificHost ? "Llms.txt content was updated for the specific host name." : "Llms.txt content was created for the specific host name.",
                    Data = saveModel
                });
            }

            return Json(new
            {
                Success = false,
                Message = "Could not perform this update to an llms.txt configuration"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error was encountered while attempting to save llms.txt content tool.");
            throw;
        }
    }
}