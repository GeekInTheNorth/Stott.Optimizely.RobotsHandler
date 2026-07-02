using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using Moq;

using NUnit.Framework;

using Stott.Optimizely.RobotsHandler.Applications;

namespace Stott.Optimizely.RobotsHandler.Test.Applications;

[TestFixture]
public sealed class ApplicationDefinitionControllerTests
{
    private Mock<IApplicationDefinitionService> _mockService;

    private ApplicationDefinitionController _controller;

    [SetUp]
    public void SetUp()
    {
        _mockService = new Mock<IApplicationDefinitionService>();
        _mockService.Setup(x => x.GetAllApplicationsAsync()).ReturnsAsync([]);

        _controller = new ApplicationDefinitionController(_mockService.Object);
    }

    [Test]
    public async Task Applications_RetrievesDataFromTheService()
    {
        // Act
        await _controller.Applications();

        // Assert
        _mockService.Verify(x => x.GetAllApplicationsAsync(), Times.Once);
    }

    [Test]
    public async Task Applications_ReturnsTheApplicationsSerialisedAsJson()
    {
        // Arrange
        var applications = new List<ApplicationViewModel>
        {
            new() { AppId = "app-one", AppName = "App One" }
        };
        _mockService.Setup(x => x.GetAllApplicationsAsync()).ReturnsAsync(applications);

        // Act
        var result = await _controller.Applications();

        // Assert
        Assert.That(result, Is.AssignableFrom<ContentResult>());
        var content = ((ContentResult)result).Content;
        Assert.That(content, Does.Contain("\"appId\":\"app-one\""));
        Assert.That(content, Does.Contain("\"appName\":\"App One\""));
    }
}
