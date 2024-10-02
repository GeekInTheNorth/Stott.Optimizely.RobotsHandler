using System.Collections.Generic;

namespace Stott.Optimizely.RobotsHandler.Environments;

public interface IEnvironmentRobotsService
{
    IList<EnvironmentRobotsModel> GetAll();

    EnvironmentRobotsModel Get(string environmentName);

    EnvironmentRobotsModel GetCurrent();

    void Save(EnvironmentRobotsModel model);
}
