using System;
using System.Collections.Generic;
using System.Text.Json;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Moq;

using NUnit.Framework;

using Stott.Optimizely.RobotsHandler.Llms;
using Stott.Optimizely.RobotsHandler.Opal;
using Stott.Optimizely.RobotsHandler.Opal.Models;
using Stott.Optimizely.RobotsHandler.Sites;
using Stott.Optimizely.RobotsHandler.Test.TestCases;

namespace Stott.Optimizely.RobotsHandler.Test.Opal;

[TestFixture]
public sealed class OpalLlmsApiControllerTests
{
    private Mock<ILlmsContentService> _mockService;

    private Mock<ILogger<OpalLlmsApiController>> _logger;

    private OpalLlmsApiController _controller;

    private JsonSerializerOptions _serializationOptions;

    [SetUp]
    public void Setup()
    {
        _mockService = new Mock<ILlmsContentService>();
        _logger = new Mock<ILogger<OpalLlmsApiController>>();

        _serializationOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        _controller = new OpalLlmsApiController(_mockService.Object, null, _logger.Object);
    }

    [Test]
    public void GetLlmsTxtConfigurations_WhenCalled_InvokesGetAllOnService()
    {
        // Act
        _controller.GetLlmsTxtConfigurations(new ToolRequest<GetConfigurationsQuery>());

        // Assert
        _mockService.Verify(s => s.GetAll(), Times.Once);
    }

    [Test]
    public void GetLlmsTxtConfigurations_GivenThereAreNoConfigurations_ThenAnEmptyArrayIsReturned()
    {
        // Arrange
        _mockService.Setup(s => s.GetAll()).Returns(new List<SiteLlmsViewModel>());

        // Act
        var result = _controller.GetLlmsTxtConfigurations(new ToolRequest<GetConfigurationsQuery>()) as ContentResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ContentType, Is.EqualTo("application/json"));
        Assert.That(result.Content, Is.EqualTo("[]"));
    }

    [Test]
    public void GetLlmsTxtConfigurations_GivenThereAreConfigurations_ThenContentModelsAreReturned()
    {
        // Arrange
        _mockService.Setup(s => s.GetAll()).Returns(CreateDummyData());

        // Act
        var response = _controller.GetLlmsTxtConfigurations(new ToolRequest<GetConfigurationsQuery>()) as ContentResult;
        var contentString = response?.Content ?? string.Empty;
        var content = JsonSerializer.Deserialize<List<OpalSiteContentModel>>(contentString, _serializationOptions);

        // Assert
        Assert.That(content, Is.Not.Null);
        Assert.That(content, Has.Count.EqualTo(3));
        Assert.That(content[0].SiteName, Is.EqualTo("Test One"));
        Assert.That(content[0].SpecificHost, Is.EqualTo("specific.test"));
        Assert.That(content[0].Content, Is.EqualTo("User-agent: *\nDisallow: /"));
        Assert.That(content[1].SiteName, Is.EqualTo("Test Two"));
        Assert.That(content[1].SpecificHost, Is.EqualTo("available3.test"));
        Assert.That(content[1].Content, Is.EqualTo("User-agent: *\nDisallow: /private"));
        Assert.That(content[2].SiteName, Is.EqualTo("Test Two"));
        Assert.That(content[2].SpecificHost, Is.EqualTo("available4.test"));
        Assert.That(content[2].Content, Is.EqualTo("User-agent: *\nDisallow: /private"));
    }

    [Test]
    public void GetLlmsTxtConfigurations_GivenAHostNameHasBeenProvided_AndTheHostNameDoesNotMatch_ThenANegativeResultIsReturned()
    {
        // Arrange
        _mockService.Setup(s => s.GetAll()).Returns(CreateDummyData());

        var model = new ToolRequest<GetConfigurationsQuery>
        {
            Parameters = new GetConfigurationsQuery
            {
                HostName = "nonexistent.test"
            }
        };

        // Act 
        var response = _controller.GetLlmsTxtConfigurations(model) as JsonResult;
        var contentString = response?.Value ?? string.Empty;

        // Assert
        Assert.That(contentString, Is.Not.Null);
        Assert.That(((dynamic)contentString).Success, Is.False);
        Assert.That(((dynamic)contentString).Message, Is.EqualTo("Could not locate a llms.txt config that matched the host name of nonexistent.test."));
    }

    [Test]
    public void GetLlmsTxtConfigurations_GivenHostNameMatchesSpecificHost_ThenSpecificConfigurationIsReturned()
    {
        // Arrange
        _mockService.Setup(s => s.GetAll()).Returns(CreateDummyData());

        var model = new ToolRequest<GetConfigurationsQuery>
        {
            Parameters = new GetConfigurationsQuery
            {
                HostName = "specific.test"
            }
        };

        // Act
        var response = _controller.GetLlmsTxtConfigurations(model) as ContentResult;
        var contentString = response?.Content ?? string.Empty;
        var content = JsonSerializer.Deserialize<OpalSiteContentModel>(contentString, _serializationOptions);

        // Assert
        Assert.That(content, Is.Not.Null);
        Assert.That(content.SiteName, Is.EqualTo("Test One"));
        Assert.That(content.SpecificHost, Is.EqualTo("specific.test"));
        Assert.That(content.Content, Is.EqualTo("User-agent: *\nDisallow: /"));
    }

    [Test]
    public void GetLlmsTxtConfigurations_GivenHostNameMatchesAvailableHost_ThenMatchingConfigurationIsReturned()
    {
        // Arrange
        _mockService.Setup(s => s.GetAll()).Returns(CreateDummyData());

        var model = new ToolRequest<GetConfigurationsQuery>
        {
            Parameters = new GetConfigurationsQuery
            {
                HostName = "available3.test"
            }
        };

        // Act
        var response = _controller.GetLlmsTxtConfigurations(model) as ContentResult;
        var contentString = response?.Content ?? string.Empty;
        var content = JsonSerializer.Deserialize<OpalSiteContentModel>(contentString, _serializationOptions);

        // Assert
        Assert.That(content, Is.Not.Null);
        Assert.That(content.SiteName, Is.EqualTo("Test Two"));
        Assert.That(content.SpecificHost, Is.EqualTo("available3.test"));
        Assert.That(content.Content, Is.EqualTo("User-agent: *\nDisallow: /private"));
    }

    [Test]
    public void GetLlmsTxtConfigurations_GivenHostNameHasWhitespace_ThenItIsTrimmed()
    {
        // Arrange
        _mockService.Setup(s => s.GetAll()).Returns(CreateDummyData());

        var model = new ToolRequest<GetConfigurationsQuery>
        {
            Parameters = new GetConfigurationsQuery
            {
                HostName = "  specific.test  "
            }
        };

        // Act
        var response = _controller.GetLlmsTxtConfigurations(model) as ContentResult;
        var contentString = response?.Content ?? string.Empty;
        var content = JsonSerializer.Deserialize<OpalSiteContentModel>(contentString, _serializationOptions);

        // Assert
        Assert.That(content, Is.Not.Null);
        Assert.That(content.SiteName, Is.EqualTo("Test One"));
        Assert.That(content.SpecificHost, Is.EqualTo("specific.test"));
    }

    [Test]
    public void GetLlmsTxtConfigurations_GivenHostNameIsCaseDifferent_ThenMatchIsFoundIgnoringCase()
    {
        // Arrange
        _mockService.Setup(s => s.GetAll()).Returns(CreateDummyData());

        var model = new ToolRequest<GetConfigurationsQuery>
        {
            Parameters = new GetConfigurationsQuery
            {
                HostName = "SPECIFIC.TEST"
            }
        };

        // Act
        var response = _controller.GetLlmsTxtConfigurations(model) as ContentResult;
        var contentString = response?.Content ?? string.Empty;
        var content = JsonSerializer.Deserialize<OpalSiteContentModel>(contentString, _serializationOptions);

        // Assert
        Assert.That(content, Is.Not.Null);
        Assert.That(content.SiteName, Is.EqualTo("Test One"));
    }

    [Test]
    public void GetLlmsTxtConfigurations_GivenNullModel_ThenAllConfigurationsAreReturned()
    {
        // Arrange
        _mockService.Setup(s => s.GetAll()).Returns(CreateDummyData());

        // Act
        var response = _controller.GetLlmsTxtConfigurations(null) as ContentResult;
        var contentString = response?.Content ?? string.Empty;
        var content = JsonSerializer.Deserialize<List<OpalSiteContentModel>>(contentString, _serializationOptions);

        // Assert
        Assert.That(content, Is.Not.Null);
        Assert.That(content, Has.Count.EqualTo(3));
    }

    [Test]
    public void GetLlmsTxtConfigurations_GivenModelWithNullParameters_ThenAllConfigurationsAreReturned()
    {
        // Arrange
        _mockService.Setup(s => s.GetAll()).Returns(CreateDummyData());

        var model = new ToolRequest<GetConfigurationsQuery>
        {
            Parameters = null
        };

        // Act
        var response = _controller.GetLlmsTxtConfigurations(model) as ContentResult;
        var contentString = response?.Content ?? string.Empty;
        var content = JsonSerializer.Deserialize<List<OpalSiteContentModel>>(contentString, _serializationOptions);

        // Assert
        Assert.That(content, Is.Not.Null);
        Assert.That(content, Has.Count.EqualTo(3));
    }

    [Test]
    public void GetLlmsTxtConfigurations_WhenExceptionOccurs_ThenExceptionIsRethrown()
    {
        // Arrange
        _mockService.Setup(s => s.GetAll()).Throws(new Exception("Test exception"));

        // Act & Assert
        Assert.Throws<Exception>(() => _controller.GetLlmsTxtConfigurations(new ToolRequest<GetConfigurationsQuery>()));
        _logger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An error was encountered while processing the llms-txt-configurations tool.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Test]
    public void SaveLlmsTxtConfigurations_GivenValidLlmsId_ThenConfigurationIsUpdated()
    {
        // Arrange
        var existingConfig = CreateDummyData()[0];
        var llmsId = existingConfig.Id;
        var newContent = "User-agent: GPT\nDisallow: /admin";

        _mockService.Setup(s => s.GetAll()).Returns(new List<SiteLlmsViewModel> { existingConfig });

        var model = new ToolRequest<SaveLlmsTextConfigurationsCommand>
        {
            Parameters = new SaveLlmsTextConfigurationsCommand
            {
                LlmsId = llmsId.ToString(),
                LlmsTxtContent = newContent
            }
        };

        // Act
        var response = _controller.SaveLlmsTxtConfigurations(model) as JsonResult;
        var result = response?.Value;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(((dynamic)result).Success, Is.True);
        Assert.That(((dynamic)result).Message, Is.EqualTo("Llms.txt content was saved."));
        _mockService.Verify(s => s.Save(It.Is<SaveLlmsModel>(m => 
            m.Id == llmsId && 
            m.LlmsContent == newContent &&
            m.SiteName == existingConfig.SiteName &&
            m.SiteId == existingConfig.SiteId &&
            m.SpecificHost == existingConfig.SpecificHost)), Times.Once);
    }

    [Test]
    public void SaveLlmsTxtConfigurations_GivenInvalidLlmsId_ThenErrorIsReturned()
    {
        // Arrange
        var invalidId = Guid.NewGuid();
        _mockService.Setup(s => s.GetAll()).Returns(CreateDummyData());

        var model = new ToolRequest<SaveLlmsTextConfigurationsCommand>
        {
            Parameters = new SaveLlmsTextConfigurationsCommand
            {
                LlmsId = invalidId.ToString(),
                LlmsTxtContent = "Some content"
            }
        };

        // Act
        var response = _controller.SaveLlmsTxtConfigurations(model) as JsonResult;
        var result = response?.Value;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(((dynamic)result).Success, Is.False);
        Assert.That(((dynamic)result).Message, Is.EqualTo($"Could not locate a llms.txt config that matched the Id of {invalidId}."));
        _mockService.Verify(s => s.Save(It.IsAny<SaveLlmsModel>()), Times.Never);
    }

    [Test]
    public void SaveLlmsTxtConfigurations_GivenEmptyLlmsId_AndValidHostName_ThenNewConfigurationIsCreated()
    {
        // Arrange
        var existingConfigs = CreateDummyData();
        var hostName = "available3.test";
        var newContent = "User-agent: GPT\nDisallow: /private";

        _mockService.Setup(s => s.GetAll()).Returns(existingConfigs);

        var model = new ToolRequest<SaveLlmsTextConfigurationsCommand>
        {
            Parameters = new SaveLlmsTextConfigurationsCommand
            {
                LlmsId = Guid.Empty.ToString(),
                HostName = hostName,
                LlmsTxtContent = newContent
            }
        };

        // Act
        var response = _controller.SaveLlmsTxtConfigurations(model) as JsonResult;
        var result = response?.Value;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(((dynamic)result).Success, Is.True);
        Assert.That(((dynamic)result).Message, Is.EqualTo("Llms.txt content was created for the specific host name."));
        _mockService.Verify(s => s.Save(It.Is<SaveLlmsModel>(m => 
            m.Id == Guid.Empty && 
            m.SpecificHost == hostName &&
            m.LlmsContent == newContent)), Times.Once);
    }

    [Test]
    public void SaveLlmsTxtConfigurations_GivenHostNameMatchingSpecificHost_ThenExistingConfigurationIsUpdated()
    {
        // Arrange
        var existingConfigs = CreateDummyData();
        var hostName = "specific.test"; // This matches the SpecificHost of the first config
        var newContent = "User-agent: GPT\nDisallow: /updated";

        _mockService.Setup(s => s.GetAll()).Returns(existingConfigs);

        var model = new ToolRequest<SaveLlmsTextConfigurationsCommand>
        {
            Parameters = new SaveLlmsTextConfigurationsCommand
            {
                HostName = hostName,
                LlmsTxtContent = newContent
            }
        };

        // Act
        var response = _controller.SaveLlmsTxtConfigurations(model) as JsonResult;
        var result = response?.Value;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(((dynamic)result).Success, Is.True);
        Assert.That(((dynamic)result).Message, Is.EqualTo("Llms.txt content was updated for the specific host name."));
        _mockService.Verify(s => s.Save(It.Is<SaveLlmsModel>(m => 
            m.Id == existingConfigs[0].Id && 
            m.SpecificHost == hostName &&
            m.LlmsContent == newContent)), Times.Once);
    }

    [Test]
    [TestCaseSource(typeof(CommonTestCases), nameof(CommonTestCases.EmptyStrings))]
    public void SaveLlmsTxtConfigurations_GivenNullEmptyOrWhitespaceLlmsTxtContent_ThenErrorIsReturned(string llmsTxtContent)
    {
        // Arrange
        var model = new ToolRequest<SaveLlmsTextConfigurationsCommand>
        {
            Parameters = new SaveLlmsTextConfigurationsCommand
            {
                LlmsId = Guid.NewGuid().ToString(),
                LlmsTxtContent = llmsTxtContent
            }
        };

        // Act
        var response = _controller.SaveLlmsTxtConfigurations(model) as JsonResult;
        var result = response?.Value;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(((dynamic)result).Success, Is.False);
        Assert.That(((dynamic)result).Message, Is.EqualTo("Llms.txt content was not provided."));
    }

    [Test]
    [TestCaseSource(typeof(CommonTestCases), nameof(CommonTestCases.InvalidGuidStrings))]
    public void SaveLlmsTxtConfigurations_GivenInvalidGuidFormat_ThenHostNamePathIsUsed(string invalidId)
    {
        // Arrange
        var existingConfigs = CreateDummyData();
        var hostName = "available3.test";
        var newContent = "User-agent: GPT\nDisallow: /private";

        _mockService.Setup(s => s.GetAll()).Returns(existingConfigs);

        var model = new ToolRequest<SaveLlmsTextConfigurationsCommand>
        {
            Parameters = new SaveLlmsTextConfigurationsCommand
            {
                LlmsId = invalidId,
                HostName = hostName,
                LlmsTxtContent = newContent
            }
        };

        // Act
        var response = _controller.SaveLlmsTxtConfigurations(model) as JsonResult;
        var result = response?.Value;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(((dynamic)result).Success, Is.True);
        _mockService.Verify(s => s.Save(It.IsAny<SaveLlmsModel>()), Times.Once);
    }

    [Test]
    public void SaveLlmsTxtConfigurations_GivenHostNameWithWhitespace_ThenItIsTrimmed()
    {
        // Arrange
        var existingConfigs = CreateDummyData();
        var hostName = "  available3.test  ";
        var newContent = "User-agent: GPT\nDisallow: /private";

        _mockService.Setup(s => s.GetAll()).Returns(existingConfigs);

        var model = new ToolRequest<SaveLlmsTextConfigurationsCommand>
        {
            Parameters = new SaveLlmsTextConfigurationsCommand
            {
                HostName = hostName,
                LlmsTxtContent = newContent
            }
        };

        // Act
        var response = _controller.SaveLlmsTxtConfigurations(model) as JsonResult;

        // Assert
        _mockService.Verify(s => s.Save(It.Is<SaveLlmsModel>(m => m.SpecificHost == "available3.test")), Times.Once);
    }

    [Test]
    public void SaveLlmsTxtConfigurations_GivenNoLlmsIdAndNoHostName_ThenErrorIsReturned()
    {
        // Arrange
        var model = new ToolRequest<SaveLlmsTextConfigurationsCommand>
        {
            Parameters = new SaveLlmsTextConfigurationsCommand
            {
                LlmsTxtContent = "Some content"
            }
        };

        // Act
        var response = _controller.SaveLlmsTxtConfigurations(model) as JsonResult;
        var result = response?.Value;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(((dynamic)result).Success, Is.False);
        Assert.That(((dynamic)result).Message, Is.EqualTo("Could not perform this update to an llms.txt configuration"));
        _mockService.Verify(s => s.Save(It.IsAny<SaveLlmsModel>()), Times.Never);
    }

    [Test]
    public void SaveLlmsTxtConfigurations_GivenNullParameters_ThenErrorIsReturned()
    {
        // Arrange
        var model = new ToolRequest<SaveLlmsTextConfigurationsCommand>
        {
            Parameters = null
        };

        // Act
        var response = _controller.SaveLlmsTxtConfigurations(model) as JsonResult;
        var result = response?.Value;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(((dynamic)result).Success, Is.False);
        Assert.That(((dynamic)result).Message, Is.EqualTo("Llms.txt content was not provided."));
    }

    [Test]
    public void SaveLlmsTxtConfigurations_WhenExceptionOccurs_ThenExceptionIsRethrown()
    {
        // Arrange
        _mockService.Setup(s => s.GetAll()).Throws(new Exception("Test exception"));

        var model = new ToolRequest<SaveLlmsTextConfigurationsCommand>
        {
            Parameters = new SaveLlmsTextConfigurationsCommand
            {
                LlmsId = Guid.NewGuid().ToString(),
                LlmsTxtContent = "Some content"
            }
        };

        // Act & Assert
        Assert.Throws<Exception>(() => _controller.SaveLlmsTxtConfigurations(model));
        _logger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An error was encountered while attempting to save llms.txt content tool.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    private static List<SiteLlmsViewModel> CreateDummyData()
    {
        return new List<SiteLlmsViewModel>
        {
            new SiteLlmsViewModel
            {
                Id = Guid.NewGuid(),
                SiteName = "Test One",
                SpecificHost = "specific.test",
                AvailableHosts = new List<SiteHostViewModel>
                {
                    new SiteHostViewModel { HostName = "available1.test" },
                    new SiteHostViewModel { HostName = "available2.test" }
                },
                LlmsContent = "User-agent: *\nDisallow: /",
                IsForWholeSite = false
            },
            new SiteLlmsViewModel
            {
                Id = Guid.NewGuid(),
                SiteName = "Test Two",
                SpecificHost = null,
                AvailableHosts = new List<SiteHostViewModel>
                {
                    new SiteHostViewModel { HostName = "available3.test" },
                    new SiteHostViewModel { HostName = "available4.test" }
                },
                LlmsContent = "User-agent: *\nDisallow: /private",
                IsForWholeSite = true
            }
        };
    }
}