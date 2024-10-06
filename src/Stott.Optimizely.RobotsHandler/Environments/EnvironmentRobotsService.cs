using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNetCore.Hosting;

using Stott.Optimizely.RobotsHandler.Cache;
using Stott.Optimizely.RobotsHandler.Common;

namespace Stott.Optimizely.RobotsHandler.Environments;

public sealed class EnvironmentRobotsService : IEnvironmentRobotsService
{
    private readonly Lazy<IEnvironmentRobotsRepository> _repository;

    private readonly IWebHostEnvironment _hostingEnvironment;

    private readonly IRobotsCacheHandler _cacheHandler;

    public EnvironmentRobotsService(
        Lazy<IEnvironmentRobotsRepository> repository,
        IWebHostEnvironment hostingEnvironment,
        IRobotsCacheHandler cacheHandler)
    {
        _repository = repository;
        _hostingEnvironment = hostingEnvironment;
        _cacheHandler = cacheHandler;
    }

    public IList<EnvironmentRobotsModel> GetAll()
    {
        var currentEnvironment = _hostingEnvironment.EnvironmentName;
        var configurations = _repository.Value.GetAll() ?? new List<EnvironmentRobotsModel>(0);
        IncludeAllEnvironments(configurations, currentEnvironment);

        var currentConfig = configurations.FirstOrDefault(x => string.Equals(x.EnvironmentName, _hostingEnvironment.EnvironmentName, StringComparison.OrdinalIgnoreCase));
        if (currentConfig != null)
        {
            currentConfig.IsCurrentEnvironment = true;
        }

        return configurations.OrderBy(x => x.EnvironmentName).ToList();
    }

    public EnvironmentRobotsModel Get(string environmentName)
    {
        if (string.IsNullOrWhiteSpace(environmentName))
        {
            return null;
        }

        var cacheKey = GetCacheKey(environmentName);
        var environmentModel = _cacheHandler.Get<EnvironmentRobotsModel>(cacheKey);
        if (environmentModel is not null)
        {
            return environmentModel;
        }

        environmentModel = _repository.Value.Get(environmentName);
        if (environmentModel is not null)
        {
            _cacheHandler.Add(cacheKey, environmentModel);
        }

        return environmentModel;
    }

    public EnvironmentRobotsModel GetCurrent()
    {
        return Get(_hostingEnvironment.EnvironmentName);
    }

    public void Save(EnvironmentRobotsModel model)
    {
        if (string.IsNullOrWhiteSpace(model?.EnvironmentName))
        {
            return;
        }

        _cacheHandler.RemoveAll();
        _repository.Value.Save(model);
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
