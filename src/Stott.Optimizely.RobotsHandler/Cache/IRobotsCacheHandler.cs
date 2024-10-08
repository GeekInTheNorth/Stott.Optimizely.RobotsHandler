namespace Stott.Optimizely.RobotsHandler.Cache;

public interface IRobotsCacheHandler
{
    void Add<T>(string cacheKey, T objectToCache) where T : class;

    T Get<T>(string cacheKey) where T : class;

    void RemoveAll();
}