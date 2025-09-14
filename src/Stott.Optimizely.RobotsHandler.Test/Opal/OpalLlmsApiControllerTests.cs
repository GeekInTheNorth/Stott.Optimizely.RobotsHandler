using System;
using System.Collections.Generic;
using System.Text.Json;

using Azure;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Moq;

using NUnit.Framework;

using Stott.Optimizely.RobotsHandler.Llms;
using Stott.Optimizely.RobotsHandler.Opal;
using Stott.Optimizely.RobotsHandler.Opal.Models;
using Stott.Optimizely.RobotsHandler.Sites;

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
                LlmsContent = "User-agent: *\nDisallow: /"
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
                LlmsContent = "User-agent: *\nDisallow: /private"
            }
        };
    }
}