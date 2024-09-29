using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

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
        var configurations = _repository.Value.GetAll();
        IncludeAllEnvironments(configurations);

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

    private void IncludeAllEnvironments(IList<EnvironmentRobotsModel> environmentModels)
    {
        if (_hostingEnvironment.IsDevelopment() && !environmentModels.Any(x => string.Equals(x.EnvironmentName, RobotsConstants.EnvironmentNames.Development, StringComparison.OrdinalIgnoreCase)))
        {
            environmentModels.Add(new EnvironmentRobotsModel { Id = Guid.NewGuid(), EnvironmentName = RobotsConstants.EnvironmentNames.Development });
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
