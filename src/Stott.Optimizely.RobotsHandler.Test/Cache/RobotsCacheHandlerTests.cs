using System;

using EPiServer.Framework.Cache;

using Microsoft.Extensions.Logging;

using Moq;

using NUnit.Framework;

using Stott.Optimizely.RobotsHandler.Cache;
using Stott.Optimizely.RobotsHandler.Test.TestCases;

namespace Stott.Optimizely.RobotsHandler.Test.Cache;

[TestFixture]
public sealed class RobotsCacheHandlerTests
{
    private Mock<ISynchronizedObjectInstanceCache> _mockCache;

    private Mock<ILogger<RobotsCacheHandler>> _mockLogger;

    private RobotsCacheHandler _cacheHandler;

    [SetUp]
    public void SetUp()
    {
        _mockCache = new Mock<ISynchronizedObjectInstanceCache>();
        _mockLogger = new Mock<ILogger<RobotsCacheHandler>>();

        _cacheHandler = new RobotsCacheHandler(_mockCache.Object, _mockLogger.Object);
    }

    [Test]
    [TestCaseSource(typeof(CommonTestCases), nameof(CommonTestCases.EmptyStrings))]
    public void Add_GivenAnInvalidCacheKey_ThenNoCacheInsertionIsAttempted(string cacheKey)
    {
        // Arrange
        var objectToCache = new object();

        // Act
        _cacheHandler.Add(cacheKey, objectToCache);

        // Assert
        _mockCache.Verify(cache => cache.Insert(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CacheEvictionPolicy>()), Times.Never);
    }

    [Test]
    public void Add_GivenANullObjectToCache_ThenNoCacheInsertionIsAttempted()
    {
        // Act
        _cacheHandler.Add<object>("cacheKey", null);

        // Assert
        _mockCache.Verify(cache => cache.Insert(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CacheEvictionPolicy>()), Times.Never);
    }

    [Test]
    public void Add_GivenAValidCacheKeyAndObjectToCache_ThenCacheInsertionIsAttempted()
    {
        // Arrange
        var cacheKey = "cacheKey";
        var objectToCache = new object();

        // Act
        _cacheHandler.Add(cacheKey, objectToCache);

        // Assert
        _mockCache.Verify(cache => cache.Insert(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CacheEvictionPolicy>()), Times.Once);
    }

    [Test]
    public void RemoveAll_ThenCacheRemovalIsAttempted()
    {
        // Act
        _cacheHandler.RemoveAll();

        // Assert
        _mockCache.Verify(cache => cache.Remove(It.IsAny<string>()), Times.Once);
    }
}
