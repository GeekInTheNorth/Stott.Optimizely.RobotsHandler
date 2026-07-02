using System;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Moq;

using NUnit.Framework;

using Stott.Optimizely.RobotsHandler.QueryRules;
using Stott.Optimizely.RobotsHandler.Test.TestCases;

namespace Stott.Optimizely.RobotsHandler.Test.QueryRules;

[TestFixture]
public sealed class QueryRulesApiControllerTests
{
    private Mock<IQueryRulesService> _mockService;

    private Mock<ILogger<QueryRulesApiController>> _mockLogger;

    private QueryRulesApiController _controller;

    [SetUp]
    public void SetUp()
    {
        _mockService = new Mock<IQueryRulesService>();

        _mockLogger = new Mock<ILogger<QueryRulesApiController>>();

        _controller = new QueryRulesApiController(_mockService.Object, _mockLogger.Object);
    }

    [Test]
    public void ApiList_RetrievesDataFromTheService()
    {
        // Act
        _controller.ApiList();

        // Assert
        _mockService.Verify(x => x.GetAll(), Times.Once);
    }

    [Test]
    [TestCaseSource(typeof(CommonTestCases), nameof(CommonTestCases.InvalidGuidStrings))]
    public void Details_WhenPresentedWithAnInvalidOrEmptyId_ThrowsArgumentException(string id)
    {
        // Assert
        Assert.Throws<ArgumentException>(() => _controller.Details(id));
    }

    [Test]
    [TestCaseSource(typeof(CommonTestCases), nameof(CommonTestCases.InvalidGuidStrings))]
    public void Details_WhenPresentedWithAnInvalidOrEmptyId_DoesNotQueryTheService(string id)
    {
        // Act
        try
        {
            _controller.Details(id);
        }
        catch (ArgumentException)
        {
            // Expected - the assertion of the throw is covered elsewhere.
        }

        // Assert
        _mockService.Verify(x => x.Get(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public void Details_WhenPresentedWithAValidId_RetrievesTheRuleFromTheService()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        _controller.Details(id.ToString());

        // Assert
        _mockService.Verify(x => x.Get(id), Times.Once);
    }

    [Test]
    public void Save_ReturnsConflictResultWhenConflictExists()
    {
        // Arrange
        var formSubmitModel = new SaveQueryRuleMode { QueryName = "utm_source" };

        _mockService.Setup(x => x.DoesConflictExists(It.IsAny<SaveQueryRuleMode>())).Returns(true);

        // Act
        var result = _controller.Save(formSubmitModel);

        // Assert
        Assert.That(result, Is.AssignableFrom<ContentResult>());
        Assert.That(((ContentResult)result).StatusCode, Is.EqualTo(409));
    }

    [Test]
    public void Save_DoesNotSaveModelWhenConflictExists()
    {
        // Arrange
        var formSubmitModel = new SaveQueryRuleMode { QueryName = "utm_source" };

        _mockService.Setup(x => x.DoesConflictExists(It.IsAny<SaveQueryRuleMode>())).Returns(true);

        // Act
        _controller.Save(formSubmitModel);

        // Assert
        _mockService.Verify(x => x.Save(It.IsAny<SaveQueryRuleMode>()), Times.Never);
    }

    [Test]
    public void Save_SavesModelWhenNoConflictExists()
    {
        // Arrange
        var formSubmitModel = new SaveQueryRuleMode { QueryName = "utm_source" };

        _mockService.Setup(x => x.DoesConflictExists(It.IsAny<SaveQueryRuleMode>())).Returns(false);

        // Act
        var result = _controller.Save(formSubmitModel);

        // Assert
        _mockService.Verify(x => x.Save(It.IsAny<SaveQueryRuleMode>()), Times.Once);
        Assert.That(result, Is.AssignableFrom<OkResult>());
    }

    [Test]
    public void Save_WhenDoesConflictExistsThrowsAnException_ThenAnInternalServerErrorIsReturned()
    {
        // Arrange
        var formSubmitModel = new SaveQueryRuleMode { QueryName = "utm_source" };

        _mockService.Setup(x => x.DoesConflictExists(It.IsAny<SaveQueryRuleMode>())).Throws<Exception>();

        // Act
        var result = _controller.Save(formSubmitModel);

        // Assert
        Assert.That(result, Is.AssignableFrom<ContentResult>());
        Assert.That(((ContentResult)result).StatusCode, Is.EqualTo(500));
    }

    [Test]
    public void Save_WhenSaveOnTheServiceThrowsAnException_ThenAnInternalServerErrorIsReturned()
    {
        // Arrange
        var formSubmitModel = new SaveQueryRuleMode { QueryName = "utm_source" };

        _mockService.Setup(x => x.DoesConflictExists(It.IsAny<SaveQueryRuleMode>())).Returns(false);
        _mockService.Setup(x => x.Save(It.IsAny<SaveQueryRuleMode>())).Throws<Exception>();

        // Act
        var result = _controller.Save(formSubmitModel);

        // Assert
        Assert.That(result, Is.AssignableFrom<ContentResult>());
        Assert.That(((ContentResult)result).StatusCode, Is.EqualTo(500));
    }

    [Test]
    public void Delete_WhenGivenAnEmptyId_ReturnsABadRequest()
    {
        // Act
        var result = _controller.Delete(Guid.Empty);

        // Assert
        Assert.That(result, Is.AssignableFrom<ContentResult>());
        Assert.That(((ContentResult)result).StatusCode, Is.EqualTo(400));
    }

    [Test]
    public void Delete_WhenGivenAnEmptyId_DoesNotCallDeleteOnTheService()
    {
        // Act
        _controller.Delete(Guid.Empty);

        // Assert
        _mockService.Verify(x => x.Delete(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public void Delete_WhenGivenAValidId_CallsDeleteOnTheService()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var result = _controller.Delete(id);

        // Assert
        _mockService.Verify(x => x.Delete(id), Times.Once);
        Assert.That(result, Is.AssignableFrom<OkResult>());
    }

    [Test]
    public void Delete_WhenServiceThrowsAnException_ReturnsInternalServerError()
    {
        // Arrange
        var id = Guid.NewGuid();

        _mockService.Setup(x => x.Delete(It.IsAny<Guid>())).Throws<Exception>();

        // Act
        var result = _controller.Delete(id);

        // Assert
        Assert.That(result, Is.AssignableFrom<ContentResult>());
        Assert.That(((ContentResult)result).StatusCode, Is.EqualTo(500));
    }
}
