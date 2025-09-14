using System;
using System.Collections.Generic;
using System.Text.Json;

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

    private JsonSerializerOptions _serializationOptions;

    [SetUp]
    public void Setup()
    {
        _mockRepository = new Mock<IOpalTokenRepository>();
        _mockLogger = new Mock<ILogger<OpalTokenController>>();

        _serializationOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

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
    public void List_GivenThereAreOneOrMoreTokens_ThenTokenValuesWillBeOfuscated()
    {
        // Arrange
        var data = new List<TokenModel>
        {
            new TokenModel { Id = Guid.NewGuid(), Token = null, Name = "Test One" },
            new TokenModel { Id = Guid.NewGuid(), Token = "Token", Name = "Test Two" },
            new TokenModel { Id = Guid.NewGuid(), Token = "TokenValueBeyondSixCharacters", Name = "Test Three" }
        };

        _mockRepository.Setup(r => r.List()).Returns(data);

        // Act
        var response = _controller.List() as ContentResult;
        var contentString = response?.Content ?? string.Empty;
        var content = JsonSerializer.Deserialize<List<TokenModel>>(contentString, _serializationOptions);

        // Assert
        Assert.That(content, Is.Not.Null);
        Assert.That(content, Has.Count.EqualTo(3));
        Assert.That(content?[0].Token, Is.Null);
        Assert.That(content?[0].Name, Is.EqualTo("Test One"));
        Assert.That(content?[1].Token, Is.EqualTo("Token"));
        Assert.That(content?[1].Name, Is.EqualTo("Test Two"));
        Assert.That(content?[2].Token, Is.EqualTo("TokenV..."));
        Assert.That(content?[2].Name, Is.EqualTo("Test Three"));
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
