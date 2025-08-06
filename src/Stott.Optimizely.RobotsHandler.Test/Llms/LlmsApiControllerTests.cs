using System;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Moq;

using NUnit.Framework;

using Stott.Optimizely.RobotsHandler.Llms;
using Stott.Optimizely.RobotsHandler.Test.TestCases;

namespace Stott.Optimizely.RobotsHandler.Test.Llms;

[TestFixture]
public sealed class LlmsApiControllerTests
{
    private Mock<ILlmsContentService> _mockService;

    private Mock<ILogger<LlmsApiController>> _mockLogger;

    private LlmsApiController _controller;

    [SetUp]
    public void SetUp()
    {
        _mockService = new Mock<ILlmsContentService>();

        _mockLogger = new Mock<ILogger<LlmsApiController>>();

        _controller = new LlmsApiController(_mockService.Object, _mockLogger.Object);
    }

    [Test]
    public void ApiList_RetrievesDataFromTheRepository()
    {
        // Act
        _controller.ApiList();

        // Assert
        _mockService.Verify(x => x.GetAll(), Times.Once);
    }

    [Test]
    [TestCaseSource(typeof(CommonTestCases), nameof(CommonTestCases.InvalidGuidStrings))]
    public void Details_ThrowsArgumentExceptionWhenPresentedWithAnInvalidSiteId(string siteId)
    {
        // Assert
        Assert.Throws<ArgumentException>(() => _controller.Details(Guid.NewGuid().ToString(), siteId));
    }

    [Test]
    public void Details_RetrievesModelWhenPresentedWithAValidSiteId()
    {
        // Arrange
        var siteId = Guid.NewGuid().ToString();

        // Act
        _controller.Details(Guid.NewGuid().ToString(), siteId);

        // Assert
        _mockService.Verify(x => x.Get(It.IsAny<Guid>()), Times.Once);
    }

    [Test]
    public void Details_RetrievesDefaultModelWhenPresentedWithAnEmptyId()
    {
        // Arrange
        var siteId = Guid.NewGuid().ToString();

        // Act
        _controller.Details(Guid.Empty.ToString(), siteId);

        // Assert
        _mockService.Verify(x => x.GetDefault(It.IsAny<Guid>()), Times.Once);
    }

    [Test]
    public void Save_ReturnsConflictResultWhenConflictExists()
    {
        // Arrange
        var formSubmitModel = new SaveLlmsModel();

        _mockService.Setup(x => x.DoesConflictExists(It.IsAny<SaveLlmsModel>())).Returns(true);

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
        var formSubmitModel = new SaveLlmsModel();

        _mockService.Setup(x => x.DoesConflictExists(It.IsAny<SaveLlmsModel>())).Returns(true);

        // Act
        _controller.Save(formSubmitModel);

        // Assert
        _mockService.Verify(x => x.Save(It.IsAny<SaveLlmsModel>()), Times.Never);
    }

    [Test]
    public void Save_SavesModelWhenNoConflictExists()
    {
        // Arrange
        var formSubmitModel = new SaveLlmsModel();

        _mockService.Setup(x => x.DoesConflictExists(It.IsAny<SaveLlmsModel>())).Returns(false);

        // Act
        _controller.Save(formSubmitModel);

        // Assert
        _mockService.Verify(x => x.Save(It.IsAny<SaveLlmsModel>()), Times.Once);
    }

    [Test]
    public void Save_WhenDoesConflictExistsThrowsAnException_ThenAnInternalServerErrorIsReturned()
    {
        // Arrange
        var formSubmitModel = new SaveLlmsModel();

        _mockService.Setup(x => x.DoesConflictExists(It.IsAny<SaveLlmsModel>())).Throws<Exception>();

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
        var formSubmitModel = new SaveLlmsModel();

        _mockService.Setup(x => x.DoesConflictExists(It.IsAny<SaveLlmsModel>())).Returns(false);
        _mockService.Setup(x => x.Save(It.IsAny<SaveLlmsModel>())).Throws<Exception>();

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
    public void Delete_WhenGivenAValidId_CallsDeleteOnTheService()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        _controller.Delete(id);

        // Assert
        _mockService.Verify(x => x.Delete(It.IsAny<Guid>()), Times.Once);
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