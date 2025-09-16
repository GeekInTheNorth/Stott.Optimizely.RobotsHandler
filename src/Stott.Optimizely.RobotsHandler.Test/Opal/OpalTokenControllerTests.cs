using System;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Moq;

using NUnit.Framework;

using Stott.Optimizely.RobotsHandler.Opal;

namespace Stott.Optimizely.RobotsHandler.Test.Opal;

[TestFixture]
public sealed class OpalTokenControllerTests
{
    private Mock<IOpalTokenRepository> _mockRepository;

    private Mock<ILogger<OpalTokenController>> _mockLogger;

    private OpalTokenController _controller;

    [SetUp]
    public void Setup()
    {
        _mockRepository = new Mock<IOpalTokenRepository>();
        _mockLogger = new Mock<ILogger<OpalTokenController>>();
        _controller = new OpalTokenController(_mockRepository.Object, _mockLogger.Object);
    }

    [Test]
    public void List_WhenCalled_InvokesListOnRepository()
    {
        // Act
        _controller.List();

        // Assert
        _mockRepository.Verify(r => r.List(), Times.Once);
    }

    [Test]
    public void Save_WhenCalled_InvokesSaveOnRepository()
    {
        // Arrange
        var model = new TokenModel { Id = Guid.NewGuid(), Token = Guid.NewGuid().ToString(), Name = "Test" };

        // Act
        var result = _controller.Save(model);

        // Assert
        _mockRepository.Verify(r => r.Save(model), Times.Once);
    }

    [Test]
    public void Delete_GivenIdIsEmpty_ReturnsBadRequest()
    {
        // Arrange
        var id = Guid.Empty;
        
        // Act
        var result = _controller.Delete(id) as ContentResult;
        
        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result?.StatusCode, Is.EqualTo(400));
        Assert.That(result?.Content, Is.EqualTo("Id must not be empty."));
    }

    [Test]
    public void Delete_GivenIdIsNotEmpty_InvokesDeleteOnRepository()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var result = _controller.Delete(id);

        // Assert
        _mockRepository.Verify(r => r.Delete(id), Times.Once);
        Assert.That(result, Is.TypeOf<OkResult>());
    }

    [Test]
    public void Delete_WhenExceptionThrown_ThenLogsErrorAndReturnsServerError()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepository.Setup(r => r.Delete(It.IsAny<Guid>())).Throws<Exception>();
        
        // Act
        var result = _controller.Delete(id) as ContentResult;
        
        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result?.StatusCode, Is.EqualTo(500));
    }
}
