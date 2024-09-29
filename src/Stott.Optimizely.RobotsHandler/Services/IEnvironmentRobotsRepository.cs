using System.Collections.Generic;

namespace Stott.Optimizely.RobotsHandler.Services;

public interface IEnvironmentRobotsRepository
{
    IList<EnvironmentRobotsModel> GetAll();

    EnvironmentRobotsModel Get(string environmentName);

    void Save(EnvironmentRobotsModel model);
}
