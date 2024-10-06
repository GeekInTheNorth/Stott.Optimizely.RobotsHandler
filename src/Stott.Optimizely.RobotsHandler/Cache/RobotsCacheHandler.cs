using System;
using System.Linq;

using EPiServer.Framework.Cache;

using Microsoft.Extensions.Logging;

namespace Stott.Optimizely.RobotsHandler.Cache;

public sealed class RobotsCacheHandler : IRobotsCacheHandler
{
    private readonly ISynchronizedObjectInstanceCache _cache;

    private readonly ILogger<RobotsCacheHandler> _logger;

    private const string MasterKey = "Stott-RobotsHandler-MasterKey";

    public RobotsCacheHandler(ISynchronizedObjectInstanceCache cache, ILogger<RobotsCacheHandler> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public void Add<T>(string cacheKey, T objectToCache)
        where T : class
    {
        if (string.IsNullOrWhiteSpace(cacheKey) || objectToCache == null)
        {
            return;
        }

        try
        {
            var evictionPolicy = new CacheEvictionPolicy(
                TimeSpan.FromHours(12),
                CacheTimeoutType.Absolute,
                Enumerable.Empty<string>(),
                new[] { MasterKey });

            _cache.Insert(cacheKey, objectToCache, evictionPolicy);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "[Robots Handler] Failed to add item to cache with a key of {cacheKey}.", cacheKey);
        }
    }

    public T Get<T>(string cacheKey)
        where T : class
    {
        if (string.IsNullOrWhiteSpace(cacheKey))
        {
            return null;
        }

        return _cache.TryGet<T>(cacheKey, ReadStrategy.Wait, out var cachedObject) ? cachedObject : default;
    }

    public void RemoveAll()
    {
        try
        {
            _cache.Remove(MasterKey);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "[Robots Handler] Failed to remove all items from cache based on the master key.");
        }
    }
}
