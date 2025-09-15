using System;
using System.Collections.Generic;
using System.Text.Json;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Moq;

using NUnit.Framework;

using Stott.Optimizely.RobotsHandler.Opal;
using Stott.Optimizely.RobotsHandler.Opal.Models;
using Stott.Optimizely.RobotsHandler.Robots;
using Stott.Optimizely.RobotsHandler.Sites;
using Stott.Optimizely.RobotsHandler.Test.TestCases;

namespace Stott.Optimizely.RobotsHandler.Test.Opal;

[TestFixture]
public sealed class OpalRobotsApiControllerTests
{
    private Mock<IRobotsContentService> _mockService;

    private Mock<ILogger<OpalRobotsApiController>> _logger;

    private OpalRobotsApiController _controller;

    private JsonSerializerOptions _serializationOptions;

    [SetUp]
    public void Setup()
    {
        _mockService = new Mock<IRobotsContentService>();
        _logger = new Mock<ILogger<OpalRobotsApiController>>();

        _serializationOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        _controller = new OpalRobotsApiController(_mockService.Object, null, _logger.Object);
    }

    [Test]
    public void GetRobotTxtConfigurations_WhenCalled_InvokesGetAllOnService()
    {
        // Act
        _controller.GetRobotTxtConfigurations(new ToolRequest<GetConfigurationsQuery>());

        // Assert
        _mockService.Verify(s => s.GetAll(), Times.Once);
    }

    [Test]
    public void GetRobotTxtConfigurations_GivenThereAreNoConfigurations_ThenAnEmptyArrayIsReturned()
    {
        // Arrange
        _mockService.Setup(s => s.GetAll()).Returns(new List<SiteRobotsViewModel>());

        // Act
        var result = _controller.GetRobotTxtConfigurations(new ToolRequest<GetConfigurationsQuery>()) as ContentResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ContentType, Is.EqualTo("application/json"));
        Assert.That(result.Content, Is.EqualTo("[]"));
    }

    [Test]
    public void GetRobotTxtConfigurations_GivenThereAreConfigurations_ThenContentModelsAreReturned()
    {
        // Arrange
        _mockService.Setup(s => s.GetAll()).Returns(CreateDummyData());

        // Act
        var response = _controller.GetRobotTxtConfigurations(new ToolRequest<GetConfigurationsQuery>()) as ContentResult;
        var contentString = response?.Content ?? string.Empty;
        var content = JsonSerializer.Deserialize<List<OpalSiteContentModel>>(contentString, _serializationOptions);

        // Assert
        Assert.That(content, Is.Not.Null);
        Assert.That(content, Has.Count.EqualTo(3));
        Assert.That(content[0].SiteName, Is.EqualTo("Test One"));
        Assert.That(content[0].SpecificHost, Is.EqualTo("specific.test"));
        Assert.That(content[0].Content, Is.EqualTo("User-agent: *\nDisallow: /admin"));
        Assert.That(content[1].SiteName, Is.EqualTo("Test Two"));
        Assert.That(content[1].SpecificHost, Is.EqualTo("available3.test"));
        Assert.That(content[1].Content, Is.EqualTo("User-agent: *\nDisallow: /private"));
        Assert.That(content[2].SiteName, Is.EqualTo("Test Two"));
        Assert.That(content[2].SpecificHost, Is.EqualTo("available4.test"));
        Assert.That(content[2].Content, Is.EqualTo("User-agent: *\nDisallow: /private"));
    }

    [Test]
    public void GetRobotTxtConfigurations_GivenAHostNameHasBeenProvided_AndTheHostNameDoesNotMatch_ThenANegativeResultIsReturned()
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
        var response = _controller.GetRobotTxtConfigurations(model) as JsonResult;
        var contentString = response?.Value ?? string.Empty;

        // Assert
        Assert.That(contentString, Is.Not.Null);
        Assert.That(((dynamic)contentString).Success, Is.False);
        Assert.That(((dynamic)contentString).Message, Is.EqualTo("Could not locate a robots.txt config that matched the host name of nonexistent.test."));
    }

    [Test]
    public void GetRobotTxtConfigurations_GivenHostNameMatchesSpecificHost_ThenSpecificConfigurationIsReturned()
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
        var response = _controller.GetRobotTxtConfigurations(model) as ContentResult;
        var contentString = response?.Content ?? string.Empty;
        var content = JsonSerializer.Deserialize<OpalSiteContentModel>(contentString, _serializationOptions);

        // Assert
        Assert.That(content, Is.Not.Null);
        Assert.That(content.SiteName, Is.EqualTo("Test One"));
        Assert.That(content.SpecificHost, Is.EqualTo("specific.test"));
        Assert.That(content.Content, Is.EqualTo("User-agent: *\nDisallow: /admin"));
    }

    [Test]
    public void GetRobotTxtConfigurations_GivenHostNameMatchesAvailableHost_ThenMatchingConfigurationIsReturned()
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
        var response = _controller.GetRobotTxtConfigurations(model) as ContentResult;
        var contentString = response?.Content ?? string.Empty;
        var content = JsonSerializer.Deserialize<OpalSiteContentModel>(contentString, _serializationOptions);

        // Assert
        Assert.That(content, Is.Not.Null);
        Assert.That(content.SiteName, Is.EqualTo("Test Two"));
        Assert.That(content.SpecificHost, Is.EqualTo("available3.test"));
        Assert.That(content.Content, Is.EqualTo("User-agent: *\nDisallow: /private"));
    }

    [Test]
    public void GetRobotTxtConfigurations_GivenHostNameHasWhitespace_ThenItIsTrimmed()
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
        var response = _controller.GetRobotTxtConfigurations(model) as ContentResult;
        var contentString = response?.Content ?? string.Empty;
        var content = JsonSerializer.Deserialize<OpalSiteContentModel>(contentString, _serializationOptions);

        // Assert
        Assert.That(content, Is.Not.Null);
        Assert.That(content.SiteName, Is.EqualTo("Test One"));
        Assert.That(content.SpecificHost, Is.EqualTo("specific.test"));
    }

    [Test]
    public void GetRobotTxtConfigurations_GivenHostNameIsCaseDifferent_ThenMatchIsFoundIgnoringCase()
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
        var response = _controller.GetRobotTxtConfigurations(model) as ContentResult;
        var contentString = response?.Content ?? string.Empty;
        var content = JsonSerializer.Deserialize<OpalSiteContentModel>(contentString, _serializationOptions);

        // Assert
        Assert.That(content, Is.Not.Null);
        Assert.That(content.SiteName, Is.EqualTo("Test One"));
    }

    [Test]
    public void GetRobotTxtConfigurations_GivenNullModel_ThenAllConfigurationsAreReturned()
    {
        // Arrange
        _mockService.Setup(s => s.GetAll()).Returns(CreateDummyData());

        // Act
        var response = _controller.GetRobotTxtConfigurations(null) as ContentResult;
        var contentString = response?.Content ?? string.Empty;
        var content = JsonSerializer.Deserialize<List<OpalSiteContentModel>>(contentString, _serializationOptions);

        // Assert
        Assert.That(content, Is.Not.Null);
        Assert.That(content, Has.Count.EqualTo(3));
    }

    [Test]
    public void GetRobotTxtConfigurations_GivenModelWithNullParameters_ThenAllConfigurationsAreReturned()
    {
        // Arrange
        _mockService.Setup(s => s.GetAll()).Returns(CreateDummyData());

        var model = new ToolRequest<GetConfigurationsQuery>
        {
            Parameters = null
        };

        // Act
        var response = _controller.GetRobotTxtConfigurations(model) as ContentResult;
        var contentString = response?.Content ?? string.Empty;
        var content = JsonSerializer.Deserialize<List<OpalSiteContentModel>>(contentString, _serializationOptions);

        // Assert
        Assert.That(content, Is.Not.Null);
        Assert.That(content, Has.Count.EqualTo(3));
    }

    [Test]
    public void GetRobotTxtConfigurations_WhenExceptionOccurs_ThenExceptionIsRethrown()
    {
        // Arrange
        _mockService.Setup(s => s.GetAll()).Throws(new Exception("Test exception"));

        // Act & Assert
        Assert.Throws<Exception>(() => _controller.GetRobotTxtConfigurations(new ToolRequest<GetConfigurationsQuery>()));
        _logger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An error was encountered while processing the robot-txt-configurations tool.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Test]
    public void SaveRobotTxtConfigurations_GivenValidRobotsId_ThenConfigurationIsUpdated()
    {
        // Arrange
        var existingConfig = CreateDummyData()[0];
        var robotsId = existingConfig.Id;
        var newContent = "User-agent: Googlebot\nDisallow: /admin";

        _mockService.Setup(s => s.GetAll()).Returns(new List<SiteRobotsViewModel> { existingConfig });

        var model = new ToolRequest<SaveRobotTextConfigurationsCommand>
        {
            Parameters = new SaveRobotTextConfigurationsCommand
            {
                RobotsId = robotsId.ToString(),
                RobotsTxtContent = newContent
            }
        };

        // Act
        var response = _controller.SaveRobotTxtConfigurations(model) as JsonResult;
        var result = response?.Value;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(((dynamic)result).Success, Is.True);
        Assert.That(((dynamic)result).Message, Is.EqualTo("Robots content was saved."));
        _mockService.Verify(s => s.Save(It.Is<SaveRobotsModel>(m => 
            m.Id == robotsId && 
            m.RobotsContent == newContent &&
            m.SiteName == existingConfig.SiteName &&
            m.SiteId == existingConfig.SiteId &&
            m.SpecificHost == existingConfig.SpecificHost)), Times.Once);
    }

    [Test]
    [TestCaseSource(typeof(CommonTestCases), nameof(CommonTestCases.InvalidGuidStrings))]
    public void SaveRobotTxtConfigurations_GivenInvalidRobotsId_ThenErrorIsReturned(string invalidId)
    {
        // Arrange
        _mockService.Setup(s => s.GetAll()).Returns(CreateDummyData());

        var model = new ToolRequest<SaveRobotTextConfigurationsCommand>
        {
            Parameters = new SaveRobotTextConfigurationsCommand
            {
                RobotsId = invalidId,
                RobotsTxtContent = "Some content"
            }
        };

        // Act
        var response = _controller.SaveRobotTxtConfigurations(model) as JsonResult;
        var result = response?.Value;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(((dynamic)result).Success, Is.False);
        _mockService.Verify(s => s.Save(It.IsAny<SaveRobotsModel>()), Times.Never);
    }

    [Test]
    [TestCaseSource(typeof(CommonTestCases), nameof(CommonTestCases.InvalidGuidStrings))]
    public void SaveRobotTxtConfigurations_GivenEmptyRobotsIdAndValidHostName_ThenNewConfigurationIsCreated(string invalidId)
    {
        // Arrange
        var hostName = "available3.test";
        var newContent = "User-agent: Googlebot\nDisallow: /private";

        _mockService.Setup(s => s.GetAll()).Returns(CreateDummyData());

        var model = new ToolRequest<SaveRobotTextConfigurationsCommand>
        {
            Parameters = new SaveRobotTextConfigurationsCommand
            {
                RobotsId = invalidId,
                HostName = hostName,
                RobotsTxtContent = newContent
            }
        };

        // Act
        var response = _controller.SaveRobotTxtConfigurations(model) as JsonResult;
        var result = response?.Value;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(((dynamic)result).Success, Is.True);
        Assert.That(((dynamic)result).Message, Is.EqualTo("Robots content was created for the specific host name."));
        _mockService.Verify(s => s.Save(It.Is<SaveRobotsModel>(m => 
            m.Id == Guid.Empty && 
            m.SpecificHost == hostName &&
            m.RobotsContent == newContent)), Times.Once);
    }

    [Test]
    public void SaveRobotTxtConfigurations_GivenHostNameMatchingSpecificHost_ThenExistingConfigurationIsUpdated()
    {
        // Arrange
        var existingConfigs = CreateDummyData();
        var hostName = "specific.test"; // This matches the SpecificHost of the first config
        var newContent = "User-agent: Googlebot\nDisallow: /updated";

        _mockService.Setup(s => s.GetAll()).Returns(existingConfigs);

        var model = new ToolRequest<SaveRobotTextConfigurationsCommand>
        {
            Parameters = new SaveRobotTextConfigurationsCommand
            {
                HostName = hostName,
                RobotsTxtContent = newContent
            }
        };

        // Act
        var response = _controller.SaveRobotTxtConfigurations(model) as JsonResult;
        var result = response?.Value;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(((dynamic)result).Success, Is.True);
        Assert.That(((dynamic)result).Message, Is.EqualTo("Robots content was updated for the specific host name."));
        _mockService.Verify(s => s.Save(It.Is<SaveRobotsModel>(m => 
            m.Id == existingConfigs[0].Id && 
            m.SpecificHost == hostName &&
            m.RobotsContent == newContent)), Times.Once);
    }

    [Test]
    [TestCaseSource(typeof(CommonTestCases), nameof(CommonTestCases.EmptyStrings))]
    public void SaveRobotTxtConfigurations_GivenNullOrEmptyRobotsTxtContent_ThenErrorIsReturned(string invalidContent)
    {
        // Arrange
        var model = new ToolRequest<SaveRobotTextConfigurationsCommand>
        {
            Parameters = new SaveRobotTextConfigurationsCommand
            {
                RobotsId = Guid.NewGuid().ToString(),
                RobotsTxtContent = invalidContent
            }
        };

        // Act
        var response = _controller.SaveRobotTxtConfigurations(model) as JsonResult;
        var result = response?.Value;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(((dynamic)result).Success, Is.False);
        Assert.That(((dynamic)result).Message, Is.EqualTo("Robots.txt content was not provided."));
        _mockService.Verify(s => s.Save(It.IsAny<SaveRobotsModel>()), Times.Never);
    }

    [Test]
    public void SaveRobotTxtConfigurations_GivenHostNameWithWhitespace_ThenItIsTrimmed()
    {
        // Arrange
        var hostName = "  available3.test  ";
        var newContent = "User-agent: Googlebot\nDisallow: /private";

        _mockService.Setup(s => s.GetAll()).Returns(CreateDummyData());

        var model = new ToolRequest<SaveRobotTextConfigurationsCommand>
        {
            Parameters = new SaveRobotTextConfigurationsCommand
            {
                HostName = hostName,
                RobotsTxtContent = newContent
            }
        };

        // Act
        var response = _controller.SaveRobotTxtConfigurations(model) as JsonResult;

        // Assert
        _mockService.Verify(s => s.Save(It.Is<SaveRobotsModel>(m => 
            m.SpecificHost == "available3.test")), Times.Once);
    }

    [Test]
    public void SaveRobotTxtConfigurations_GivenNoRobotsIdAndNoHostName_ThenErrorIsReturned()
    {
        // Arrange
        var model = new ToolRequest<SaveRobotTextConfigurationsCommand>
        {
            Parameters = new SaveRobotTextConfigurationsCommand
            {
                RobotsTxtContent = "Some content"
            }
        };

        // Act
        var response = _controller.SaveRobotTxtConfigurations(model) as JsonResult;
        var result = response?.Value;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(((dynamic)result).Success, Is.False);
        Assert.That(((dynamic)result).Message, Is.EqualTo("Could not perform this update to a robots.txt configuration"));
        _mockService.Verify(s => s.Save(It.IsAny<SaveRobotsModel>()), Times.Never);
    }

    [Test]
    public void SaveRobotTxtConfigurations_GivenNullParameters_ThenErrorIsReturned()
    {
        // Arrange
        var model = new ToolRequest<SaveRobotTextConfigurationsCommand>
        {
            Parameters = null
        };

        // Act
        var response = _controller.SaveRobotTxtConfigurations(model) as JsonResult;
        var result = response?.Value;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(((dynamic)result).Success, Is.False);
        Assert.That(((dynamic)result).Message, Is.EqualTo("Robots.txt content was not provided."));
    }

    [Test]
    public void SaveRobotTxtConfigurations_WhenExceptionOccurs_ThenExceptionIsRethrown()
    {
        // Arrange
        _mockService.Setup(s => s.GetAll()).Throws(new Exception("Test exception"));

        var model = new ToolRequest<SaveRobotTextConfigurationsCommand>
        {
            Parameters = new SaveRobotTextConfigurationsCommand
            {
                RobotsId = Guid.NewGuid().ToString(),
                RobotsTxtContent = "Some content"
            }
        };

        // Act & Assert
        Assert.Throws<Exception>(() => _controller.SaveRobotTxtConfigurations(model));
        _logger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An error was encountered while processing the robot-txt-configurations tool.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    private static List<SiteRobotsViewModel> CreateDummyData()
    {
        return new List<SiteRobotsViewModel>
        {
            new SiteRobotsViewModel
            {
                Id = Guid.NewGuid(),
                SiteName = "Test One",
                SpecificHost = "specific.test",
                AvailableHosts = new List<SiteHostViewModel>
                {
                    new SiteHostViewModel { HostName = "available1.test" },
                    new SiteHostViewModel { HostName = "available2.test" }
                },
                RobotsContent = "User-agent: *\nDisallow: /admin",
                IsForWholeSite = false
            },
            new SiteRobotsViewModel
            {
                Id = Guid.NewGuid(),
                SiteName = "Test Two",
                SpecificHost = null,
                AvailableHosts = new List<SiteHostViewModel>
                {
                    new SiteHostViewModel { HostName = "available3.test" },
                    new SiteHostViewModel { HostName = "available4.test" }
                },
                RobotsContent = "User-agent: *\nDisallow: /private",
                IsForWholeSite = true
            }
        };
    }
}
