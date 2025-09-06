using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Stott.Optimizely.RobotsHandler.Common;
using Stott.Optimizely.RobotsHandler.Llms;
using Stott.Optimizely.RobotsHandler.Opal.Models;

namespace Stott.Optimizely.RobotsHandler.Opal;

[AllowAnonymous]
public sealed class OpalLlmsApiController : BaseApiController
{
    private readonly ILlmsContentService _service;

    private readonly ILogger<OpalLlmsApiController> _logger;

    public OpalLlmsApiController(ILlmsContentService service, ILogger<OpalLlmsApiController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpPost]
    [Route("/stott.robotshandler/opal/tools/get-llms-txt-configurations/")]
    [Route("/stott.robotshandler/opal/discovery/tools/get-llms-txt-configurations/")]
    [OpalAuthorization(OpalAuthorizationLevel.Read)]
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
                        Message = $"Could not locate a llms.txt config that matched the host name of {model.Parameters.HostName}."
                    });
                }

                return CreateSafeJsonResult(ToOpalModel(specificConfiguration));
            }

            return CreateSafeJsonResult(ToOpalModels(configurations));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error was encountered while processing the robot-txt-configurations tool.");
            throw;
        }
    }

    [HttpPost]
    [Route("/stott.robotshandler/opal/tools/save-llms-txt-configuration/")]
    [Route("/stott.robotshandler/opal/discovery/tools/save-llms-txt-configuration/")]
    [OpalAuthorization(OpalAuthorizationLevel.Write)]
    public IActionResult SaveLlmsTxtConfigurations([FromBody] ToolRequest<SaveLlmsTextConfigurationsCommand> model)
    {
        try
        {
            var configurations = _service.GetAll();
            var hostName = model.Parameters?.HostName?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(model.Parameters?.LlmsTxtContent))
            {
                return Json(new
                {
                    Success = false,
                    Message = $"Llms.txt content was not provided."
                });
            }

            if (Guid.TryParse(model.Parameters?.LlmsId, out var llmsId) && !Guid.Empty.Equals(llmsId))
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
                    SiteId = specificRobotsConfig.SiteId,
                    SiteName = specificRobotsConfig.SiteName,
                    SpecificHost = specificRobotsConfig.SpecificHost,
                    LlmsContent = model.Parameters?.LlmsTxtContent ?? specificRobotsConfig.LlmsContent
                };

                _service.Save(saveModel);
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
                    configurations.FirstOrDefault(x => x.AvailableHosts.Any(h => string.Equals(h.HostName, hostName, StringComparison.OrdinalIgnoreCase)));

                var isSpecificHost = !specificConfiguration.IsForWholeSite && string.Equals(hostName, specificConfiguration.SpecificHost, StringComparison.OrdinalIgnoreCase);

                var saveModel = new SaveLlmsModel
                {
                    Id = isSpecificHost ? specificConfiguration.Id : Guid.Empty,
                    SiteId = specificConfiguration.SiteId,
                    SiteName = specificConfiguration.SiteName,
                    SpecificHost = hostName,
                    LlmsContent = model.Parameters?.LlmsTxtContent
                };

                _service.Save(saveModel);
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
            _logger.LogError(ex, "An error was encountered while attempting to save llms.txt content tool.");
            throw;
        }
    }

    private static IEnumerable<SiteLlmsOpalModel> ToOpalModels(IEnumerable<SiteLlmsViewModel> siteModels)
    {
        if (siteModels is null)
        {
            yield break;
        }

        foreach (var siteModel in siteModels)
        {
            yield return ToOpalModel(siteModel);
        }
    }

    private static SiteLlmsOpalModel ToOpalModel(SiteLlmsViewModel siteModel)
    {
        return new SiteLlmsOpalModel
        {
            Id = siteModel.Id,
            SiteName = siteModel.SiteName,
            IsDefaultForSite = siteModel.IsForWholeSite,
            SpecificHost = siteModel.SpecificHost,
            LlmsContent = siteModel.LlmsContent
        };
    }
}
