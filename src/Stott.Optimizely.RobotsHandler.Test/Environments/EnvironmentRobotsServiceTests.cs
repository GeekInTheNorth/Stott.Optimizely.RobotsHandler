using System;
using System.Linq;

using Microsoft.AspNetCore.Hosting;

using Moq;

using Newtonsoft.Json.Linq;

using NUnit.Framework;

using Stott.Optimizely.RobotsHandler.Cache;
using Stott.Optimizely.RobotsHandler.Common;
using Stott.Optimizely.RobotsHandler.Environments;
using Stott.Optimizely.RobotsHandler.Test.TestCases;

namespace Stott.Optimizely.RobotsHandler.Test.Environments;

[TestFixture]
public sealed class EnvironmentRobotsServiceTests
{
    private Mock<IEnvironmentRobotsRepository> _mockRepository;

    private Mock<IWebHostEnvironment> _mockHostingEnvironment;

    private Mock<IRobotsCacheHandler> _mockCacheHandler;

    private EnvironmentRobotsService _service;

    [SetUp]
    public void SetUp()
    {
        _mockRepository = new Mock<IEnvironmentRobotsRepository>();
        _mockHostingEnvironment = new Mock<IWebHostEnvironment>();
        _mockCacheHandler = new Mock<IRobotsCacheHandler>();

        _service = new EnvironmentRobotsService(
            new Lazy<IEnvironmentRobotsRepository>(() => _mockRepository.Object),
            _mockHostingEnvironment.Object,
            _mockCacheHandler.Object);
    }

    [Test]
    [TestCase("Test")]
    [TestCase("Development")]
    public void GetAll_WhenCalled_ReturnsAllDxpEnvironments(string currentEnvironment)
    {
        // Arrange
        _mockHostingEnvironment.Setup(x => x.EnvironmentName).Returns(currentEnvironment);

        // Act
        var result = _service.GetAll();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Has.Count.EqualTo(4));
        Assert.That(result.Any(x => x.EnvironmentName == RobotsConstants.EnvironmentNames.Integration), Is.True);
        Assert.That(result.Any(x => x.EnvironmentName == RobotsConstants.EnvironmentNames.Preproduction), Is.True);
        Assert.That(result.Any(x => x.EnvironmentName == RobotsConstants.EnvironmentNames.Production), Is.True);
    }

    [Test]
    [TestCaseSource(typeof(EnvironmentRobotsServiceTestCases), nameof(EnvironmentRobotsServiceTestCases.EnvironmentNameTestCases))]
    public void GetAll_WhenCalled_AlwaysReturnsTheCurrentEnvironmentFirst(string environmentName)
    {
        // Arrange
        _mockHostingEnvironment.Setup(x => x.EnvironmentName).Returns(environmentName);

        // Act
        var result = _service.GetAll();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.First().EnvironmentName, Is.EqualTo(environmentName));
    }

    [Test]
    [TestCaseSource(typeof(CommonTestCases), nameof(CommonTestCases.EmptyStrings))]
    public void Get_WhenPassedAnEmptyEnvironmentName_ReturnsNull(string environmentName)
    {
        // Act
        var result = _service.Get(environmentName);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    [TestCaseSource(typeof(CommonTestCases), nameof(CommonTestCases.EmptyStrings))]
    public void Get_WhenPassedAnEmptyEnvironmentName_DoesNotAttemptToLoadContent(string environmentName)
    {
        // Act
        var result = _service.Get(environmentName);

        // Assert
        _mockRepository.Verify(x => x.Get(It.IsAny<string>()), Times.Never);
        _mockCacheHandler.Verify(x => x.Get<EnvironmentRobotsModel>(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void Get_WhenPassedAValidEnvironmentName_AndTheModelIsInCache_ReturnsTheModelFromTheCache()
    {
        // Arrange
        var environmentName = "Test";
        var model = new EnvironmentRobotsModel { EnvironmentName = environmentName };

        _mockCacheHandler.Setup(x => x.Get<EnvironmentRobotsModel>(It.IsAny<string>()))
                         .Returns(model);

        // Act
        var result = _service.Get(environmentName);

        // Assert
        Assert.That(result, Is.EqualTo(model));
        _mockCacheHandler.Verify(x => x.Get<EnvironmentRobotsModel>(It.IsAny<string>()), Times.Once);
        _mockRepository.Verify(x => x.Get(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void Get_WhenPassedAValidEnvironmentName_AndTheModelIsNotInTheCache_ThenTheModelIsLoadedFromTheRepository()
    {
        // Arrange
        var environmentName = "Test";
        var model = new EnvironmentRobotsModel { EnvironmentName = environmentName };

        _mockRepository.Setup(x => x.Get(It.IsAny<string>())).Returns(model);

        // Act
        var result = _service.Get(environmentName);

        // Assert
        Assert.That(result, Is.EqualTo(model));
        _mockCacheHandler.Verify(x => x.Get<EnvironmentRobotsModel>(It.IsAny<string>()), Times.Once);
        _mockRepository.Verify(x => x.Get(It.IsAny<string>()), Times.Once);
    }

    [Test]
    [TestCaseSource(typeof(CommonTestCases), nameof(CommonTestCases.EmptyStrings))]
    public void GetCurrent_WhenPassedAnEmptyEnvironmentName_ReturnsNull(string environmentName)
    {
        // Arrange
        _mockHostingEnvironment.Setup(x => x.EnvironmentName).Returns(environmentName);

        // Act
        var result = _service.GetCurrent();

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    [TestCaseSource(typeof(CommonTestCases), nameof(CommonTestCases.EmptyStrings))]
    public void GetCurrent_WhenPassedAnEmptyEnvironmentName_DoesNotAttemptToLoadContent(string environmentName)
    {
        // Arrange
        _mockHostingEnvironment.Setup(x => x.EnvironmentName).Returns(environmentName);

        // Act
        var result = _service.GetCurrent();

        // Assert
        _mockRepository.Verify(x => x.Get(It.IsAny<string>()), Times.Never);
        _mockCacheHandler.Verify(x => x.Get<EnvironmentRobotsModel>(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void GetCurrent_WhenPassedAValidEnvironmentName_AndTheModelIsInCache_ReturnsTheModelFromTheCache()
    {
        // Arrange
        var environmentName = "Test";
        var model = new EnvironmentRobotsModel { EnvironmentName = environmentName };

        _mockHostingEnvironment.Setup(x => x.EnvironmentName).Returns(environmentName);
        _mockCacheHandler.Setup(x => x.Get<EnvironmentRobotsModel>(It.IsAny<string>())).Returns(model);

        // Act
        var result = _service.GetCurrent();

        // Assert
        Assert.That(result, Is.EqualTo(model));
        _mockCacheHandler.Verify(x => x.Get<EnvironmentRobotsModel>(It.IsAny<string>()), Times.Once);
        _mockRepository.Verify(x => x.Get(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void GetCurrent_WhenPassedAValidEnvironmentName_AndTheModelIsNotInTheCache_ThenTheModelIsLoadedFromTheRepository()
    {
        // Arrange
        var environmentName = "Test";
        var model = new EnvironmentRobotsModel { EnvironmentName = environmentName };

        _mockHostingEnvironment.Setup(x => x.EnvironmentName).Returns(environmentName);
        _mockRepository.Setup(x => x.Get(It.IsAny<string>())).Returns(model);

        // Act
        var result = _service.GetCurrent();

        // Assert
        Assert.That(result, Is.EqualTo(model));
        _mockCacheHandler.Verify(x => x.Get<EnvironmentRobotsModel>(It.IsAny<string>()), Times.Once);
        _mockRepository.Verify(x => x.Get(It.IsAny<string>()), Times.Once);
    }

    [Test]
    public void Save_WhenPassedANullModel_DoesNotAttemptToSaveOrClearCache()
    {
        // Act
        _service.Save(null);

        // Assert
        _mockRepository.Verify(x => x.Save(It.IsAny<EnvironmentRobotsModel>()), Times.Never);
        _mockCacheHandler.Verify(x => x.RemoveAll(), Times.Never);
    }

    [Test]
    [TestCaseSource(typeof(CommonTestCases), nameof(CommonTestCases.EmptyStrings))]
    public void Save_WhenPassedAModelWithAnEmptyEnvironmentName_DoesNotAttemptToSaveOrClearCache(string environmentName)
    {
        // Act
        _service.Save(new EnvironmentRobotsModel { EnvironmentName = environmentName });

        // Assert
        _mockRepository.Verify(x => x.Save(It.IsAny<EnvironmentRobotsModel>()), Times.Never);
        _mockCacheHandler.Verify(x => x.RemoveAll(), Times.Never);
    }

    [Test]
    public void Save_WhenSaveIsCalledForAValid_ThenSaveWillBeCalledOnTheRepository()
    {
        // Act
        _service.Save(new EnvironmentRobotsModel { EnvironmentName = RobotsConstants.EnvironmentNames.Production });

        // Assert
        _mockRepository.Verify(x => x.Save(It.IsAny<EnvironmentRobotsModel>()), Times.Once);
    }

    [Test]
    public void Save_WhenSaveIsCalledForAValid_ThenTheCacheIsEmpty()
    {
        // Act
        _service.Save(new EnvironmentRobotsModel { EnvironmentName = RobotsConstants.EnvironmentNames.Production });

        // Assert
        _mockRepository.Verify(x => x.Save(It.IsAny<EnvironmentRobotsModel>()), Times.Once);
    }
}