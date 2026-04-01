using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNetCore.Hosting;

using Stott.Optimizely.RobotsHandler.Cache;
using Stott.Optimizely.RobotsHandler.Common;

namespace Stott.Optimizely.RobotsHandler.Environments;

public sealed class EnvironmentRobotsService(
    Lazy<IEnvironmentRobotsRepository> repository,
    IWebHostEnvironment hostingEnvironment,
    IRobotsCacheHandler cacheHandler) : IEnvironmentRobotsService
{
    public IList<EnvironmentRobotsModel> GetAll()
    {
        var currentEnvironment = hostingEnvironment.EnvironmentName;
        var configurations = repository.Value.GetAll() ?? [];
        IncludeAllEnvironments(configurations, currentEnvironment);

        var currentConfig = configurations.FirstOrDefault(x => string.Equals(x.EnvironmentName, currentEnvironment, StringComparison.OrdinalIgnoreCase));
        currentConfig?.IsCurrentEnvironment = true;

        return [.. configurations.OrderByDescending(x => x.IsCurrentEnvironment).ThenBy(x => x.EnvironmentName)];
    }

    public EnvironmentRobotsModel? Get(string? environmentName)
    {
        if (string.IsNullOrWhiteSpace(environmentName))
        {
            return null;
        }

        var cacheKey = GetCacheKey(environmentName);
        var environmentModel = cacheHandler.Get<EnvironmentRobotsModel>(cacheKey);
        if (environmentModel is not null)
        {
            return environmentModel;
        }

        environmentModel = repository.Value.Get(environmentName);
        if (environmentModel is not null)
        {
            cacheHandler.Add(cacheKey, environmentModel);
        }

        return environmentModel;
    }

    public EnvironmentRobotsModel? GetCurrent()
    {
        return Get(hostingEnvironment.EnvironmentName);
    }

    public void Save(EnvironmentRobotsModel model)
    {
        if (string.IsNullOrWhiteSpace(model?.EnvironmentName))
        {
            return;
        }

        cacheHandler.RemoveAll();
        repository.Value.Save(model);
    }

    private static void IncludeAllEnvironments(IList<EnvironmentRobotsModel> environmentModels, string currentEnvironmentName)
    {
        if (!string.IsNullOrWhiteSpace(currentEnvironmentName) && !environmentModels.Any(x => string.Equals(x.EnvironmentName, currentEnvironmentName, StringComparison.OrdinalIgnoreCase)))
        {
            environmentModels.Add(new EnvironmentRobotsModel { Id = Guid.NewGuid(), EnvironmentName = currentEnvironmentName });
        }

        if (!environmentModels.Any(x => string.Equals(x.EnvironmentName, RobotsConstants.EnvironmentNames.Integration, StringComparison.OrdinalIgnoreCase)))
        {
            environmentModels.Add(new EnvironmentRobotsModel { Id = Guid.NewGuid(), EnvironmentName = RobotsConstants.EnvironmentNames.Integration });
        }

        if (!environmentModels.Any(x => string.Equals(x.EnvironmentName, RobotsConstants.EnvironmentNames.Preproduction, StringComparison.OrdinalIgnoreCase)))
        {
            environmentModels.Add(new EnvironmentRobotsModel { Id = Guid.NewGuid(), EnvironmentName = RobotsConstants.EnvironmentNames.Preproduction });
        }

        if (!environmentModels.Any(x => string.Equals(x.EnvironmentName, RobotsConstants.EnvironmentNames.Production, StringComparison.OrdinalIgnoreCase)))
        {
            environmentModels.Add(new EnvironmentRobotsModel { Id = Guid.NewGuid(), EnvironmentName = RobotsConstants.EnvironmentNames.Production });
        }
    }

    private static string GetCacheKey(string environmentName)
    {
        return $"Stott-RobotsHandler-Environment-{environmentName}";
    }
}
