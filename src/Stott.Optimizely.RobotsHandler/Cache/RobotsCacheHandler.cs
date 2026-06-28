using System;

using EPiServer.Framework.Cache;

using Microsoft.Extensions.Logging;

namespace Stott.Optimizely.RobotsHandler.Cache;

public sealed class RobotsCacheHandler(
    ISynchronizedObjectInstanceCache cache, 
    ILogger<RobotsCacheHandler> logger) : IRobotsCacheHandler
{
    private const string MasterKey = "Stott-RobotsHandler-MasterKey";

    public void Add<T>(string cacheKey, T objectToCache)
        where T : class
    {
        if (string.IsNullOrWhiteSpace(cacheKey) || objectToCache == null)
        {
            return;
        }

        try
        {
            var evictionPolicy = new CacheEvictionPolicy(TimeSpan.FromHours(12), CacheTimeoutType.Absolute, [], [MasterKey]);

            cache.Insert(cacheKey, objectToCache, evictionPolicy);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "[Robots Handler] Failed to add item to cache with a key of {cacheKey}.", cacheKey);
        }
    }

    public T? Get<T>(string cacheKey)
        where T : class
    {
        if (string.IsNullOrWhiteSpace(cacheKey))
        {
            return null;
        }

        return cache.TryGet<T>(cacheKey, ReadStrategy.Wait, out var cachedObject) ? cachedObject : default;
    }

    public T? Get<T>(string cacheKey, Func<T?> factory)
        where T : class
    {
        if (string.IsNullOrWhiteSpace(cacheKey))
        {
            return null;
        }

        if (cache.TryGet<T>(cacheKey, ReadStrategy.Wait, out var cachedObject))
        {
            return cachedObject;
        }

        var objectToCache = factory();
        if (objectToCache != null)
        {
            Add(cacheKey, objectToCache);
        }

        return objectToCache;
    }

    public void RemoveAll()
    {
        try
        {
            cache.Remove(MasterKey);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "[Robots Handler] Failed to remove all items from cache based on the master key.");
        }
    }
}
