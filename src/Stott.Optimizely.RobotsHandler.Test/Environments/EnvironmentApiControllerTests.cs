using System;
using System.Collections.Generic;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Moq;

using NUnit.Framework;

using Stott.Optimizely.RobotsHandler.Environments;

namespace Stott.Optimizely.RobotsHandler.Test.Environments;

[TestFixture]
public sealed class EnvironmentApiControllerTests
{
    private Mock<IEnvironmentRobotsService> _mockService;

    private Mock<ILogger<EnvironmentApiController>> _mockLogger;

    private EnvironmentApiController _controller;

    [SetUp]
    public void SetUp()
    {
        _mockService = new Mock<IEnvironmentRobotsService>();
        _mockLogger = new Mock<ILogger<EnvironmentApiController>>();

        _controller = new EnvironmentApiController(_mockService.Object, _mockLogger.Object);
    }

    [Test]
    public void List_GivenNoEnvironments_ThenReturnsEmptyList()
    {
        // Arrange
        _mockService.Setup(service => service.GetAll()).Returns(new List<EnvironmentRobotsModel>());

        // Act
        var result = _controller.List() as ContentResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Content, Is.EqualTo("[]"));
    }

    [Test]
    public void Save_GivenValidModel_ThenReturnsOkResult()
    {
        // Arrange
        var model = new EnvironmentRobotsModel();

        // Act
        var result = _controller.Save(model) as OkResult;

        // Assert
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void Save_GivenException_ThenReturnsInternalServerErrorResult()
    {
        // Arrange
        var model = new EnvironmentRobotsModel();
        _mockService.Setup(service => service.Save(model)).Throws(new Exception("Test exception"));

        // Act
        var result = _controller.Save(model) as ContentResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(500));
        Assert.That(result.Content, Is.EqualTo("Test exception"));
    }
}
