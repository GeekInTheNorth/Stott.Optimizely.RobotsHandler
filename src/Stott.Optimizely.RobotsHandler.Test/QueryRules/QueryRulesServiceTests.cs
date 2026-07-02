using System;
using System.Collections.Generic;

using Moq;

using NUnit.Framework;

using Stott.Optimizely.RobotsHandler.Cache;
using Stott.Optimizely.RobotsHandler.QueryRules;

namespace Stott.Optimizely.RobotsHandler.Test.QueryRules;

[TestFixture]
public sealed class QueryRulesServiceTests
{
    private Mock<IQueryRulesRepository> _mockRepository;

    private Mock<IRobotsCacheHandler> _mockCacheHandler;

    private QueryRulesService _service;

    [SetUp]
    public void SetUp()
    {
        _mockRepository = new Mock<IQueryRulesRepository>();
        _mockCacheHandler = new Mock<IRobotsCacheHandler>();

        // Emulate a cache miss: the cache handler defers to the factory it is given.
        _mockCacheHandler.Setup(x => x.Get(It.IsAny<string>(), It.IsAny<Func<List<IQueryStringRule>>>()))
                         .Returns((string _, Func<List<IQueryStringRule>> factory) => factory());
        _mockCacheHandler.Setup(x => x.Get(It.IsAny<string>(), It.IsAny<Func<IQueryStringRule>>()))
                         .Returns((string _, Func<IQueryStringRule> factory) => factory());

        _service = new QueryRulesService(
            new Lazy<IQueryRulesRepository>(() => _mockRepository.Object),
            _mockCacheHandler.Object);
    }

    [Test]
    public void GetAll_WhenTheCacheReturnsData_ThenTheCachedDataIsReturned()
    {
        // Arrange
        var cached = new List<IQueryStringRule> { new SaveQueryRuleMode { QueryName = "utm_source" } };
        _mockCacheHandler.Setup(x => x.Get(It.IsAny<string>(), It.IsAny<Func<List<IQueryStringRule>>>()))
                         .Returns(cached);

        // Act
        var result = _service.GetAll();

        // Assert
        Assert.That(result, Is.EqualTo(cached));
    }

    [Test]
    public void GetAll_WhenNotCached_ThenTheDataIsLoadedFromTheRepository()
    {
        // Arrange
        var rules = new List<IQueryStringRule> { new SaveQueryRuleMode { QueryName = "utm_source" } };
        _mockRepository.Setup(x => x.GetAll()).Returns(rules);

        // Act
        var result = _service.GetAll();

        // Assert
        Assert.That(result, Is.EqualTo(rules));
        _mockRepository.Verify(x => x.GetAll(), Times.Once);
    }

    [Test]
    public void GetAll_WhenTheCacheHandlerReturnsNull_ThenAnEmptyListIsReturned()
    {
        // Arrange
        _mockCacheHandler.Setup(x => x.Get(It.IsAny<string>(), It.IsAny<Func<List<IQueryStringRule>>>()))
                         .Returns((List<IQueryStringRule>)null);

        // Act
        var result = _service.GetAll();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void Get_WhenNotCached_ThenTheRuleIsLoadedFromTheRepository()
    {
        // Arrange
        var id = Guid.NewGuid();
        var rule = new SaveQueryRuleMode { Id = id, QueryName = "utm_source" };
        _mockRepository.Setup(x => x.Get(It.IsAny<Guid>())).Returns(rule);

        // Act
        var result = _service.Get(id);

        // Assert
        Assert.That(result, Is.EqualTo(rule));
        _mockRepository.Verify(x => x.Get(id), Times.Once);
    }

    [Test]
    public void Get_WhenTheCacheReturnsData_ThenTheRepositoryIsNotQueried()
    {
        // Arrange
        var id = Guid.NewGuid();
        var cached = new SaveQueryRuleMode { Id = id, QueryName = "utm_source" };
        _mockCacheHandler.Setup(x => x.Get(It.IsAny<string>(), It.IsAny<Func<IQueryStringRule>>()))
                         .Returns(cached);

        // Act
        var result = _service.Get(id);

        // Assert
        Assert.That(result, Is.EqualTo(cached));
        _mockRepository.Verify(x => x.Get(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public void Save_CallsSaveOnTheRepository()
    {
        // Arrange
        var model = new SaveQueryRuleMode { QueryName = "utm_source" };

        // Act
        _service.Save(model);

        // Assert
        _mockRepository.Verify(x => x.Save(model), Times.Once);
    }

    [Test]
    public void Save_ClearsTheCache()
    {
        // Arrange
        var model = new SaveQueryRuleMode { QueryName = "utm_source" };

        // Act
        _service.Save(model);

        // Assert
        _mockCacheHandler.Verify(x => x.RemoveAll(), Times.Once);
    }

    [Test]
    public void Delete_CallsDeleteOnTheRepository()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        _service.Delete(id);

        // Assert
        _mockRepository.Verify(x => x.Delete(id), Times.Once);
    }

    [Test]
    public void Delete_ClearsTheCache()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        _service.Delete(id);

        // Assert
        _mockCacheHandler.Verify(x => x.RemoveAll(), Times.Once);
    }

    [Test]
    public void DoesConflictExists_WhenNoRulesExist_ThenReturnsFalse()
    {
        // Arrange
        _mockRepository.Setup(x => x.GetAll()).Returns([]);
        var model = new SaveQueryRuleMode { QueryName = "utm_source", MatchRule = "Exact" };

        // Act
        var result = _service.DoesConflictExists(model);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void DoesConflictExists_WhenAMatchingRuleExistsForADifferentId_ThenReturnsTrue()
    {
        // Arrange
        var existing = new SaveQueryRuleMode { Id = Guid.NewGuid(), QueryName = "utm_source", MatchRule = "Exact" };
        _mockRepository.Setup(x => x.GetAll()).Returns([existing]);
        var model = new SaveQueryRuleMode { Id = Guid.NewGuid(), QueryName = "utm_source", MatchRule = "Exact" };

        // Act
        var result = _service.DoesConflictExists(model);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void DoesConflictExists_ComparisonIsCaseInsensitive()
    {
        // Arrange
        var existing = new SaveQueryRuleMode { Id = Guid.NewGuid(), QueryName = "UTM_SOURCE", MatchRule = "EXACT" };
        _mockRepository.Setup(x => x.GetAll()).Returns([existing]);
        var model = new SaveQueryRuleMode { Id = Guid.NewGuid(), QueryName = "utm_source", MatchRule = "exact" };

        // Act
        var result = _service.DoesConflictExists(model);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void DoesConflictExists_WhenTheMatchingRuleIsTheSameRecord_ThenReturnsFalse()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existing = new SaveQueryRuleMode { Id = id, QueryName = "utm_source", MatchRule = "Exact" };
        _mockRepository.Setup(x => x.GetAll()).Returns([existing]);
        var model = new SaveQueryRuleMode { Id = id, QueryName = "utm_source", MatchRule = "Exact" };

        // Act
        var result = _service.DoesConflictExists(model);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void DoesConflictExists_WhenOnlyTheQueryNameMatches_ThenReturnsFalse()
    {
        // Arrange
        var existing = new SaveQueryRuleMode { Id = Guid.NewGuid(), QueryName = "utm_source", MatchRule = "Exact" };
        _mockRepository.Setup(x => x.GetAll()).Returns([existing]);
        var model = new SaveQueryRuleMode { Id = Guid.NewGuid(), QueryName = "utm_source", MatchRule = "StartsWith" };

        // Act
        var result = _service.DoesConflictExists(model);

        // Assert
        Assert.That(result, Is.False);
    }
}
