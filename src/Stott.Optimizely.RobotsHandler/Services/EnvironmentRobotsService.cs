using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNetCore.Hosting;

using Stott.Optimizely.RobotsHandler.Common;

namespace Stott.Optimizely.RobotsHandler.Services;

public class EnvironmentRobotsService : IEnvironmentRobotsService
{
    private readonly Lazy<IEnvironmentRobotsRepository> _repository;

    private readonly IWebHostEnvironment _hostingEnvironment;

    public EnvironmentRobotsService(
        Lazy<IEnvironmentRobotsRepository> repository, 
        IWebHostEnvironment hostingEnvironment)
    {
        _repository = repository;
        _hostingEnvironment = hostingEnvironment;
    }

    public IList<EnvironmentRobotsModel> GetAll()
    {
        var currentEnvironment = _hostingEnvironment.EnvironmentName;
        var configurations = _repository.Value.GetAll();
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
        return _repository.Value.Get(environmentName);
    }

    public EnvironmentRobotsModel GetCurrent()
    {
        return _repository.Value.Get(_hostingEnvironment.EnvironmentName);
    }

    public void Save(EnvironmentRobotsModel model)
    {
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
}
