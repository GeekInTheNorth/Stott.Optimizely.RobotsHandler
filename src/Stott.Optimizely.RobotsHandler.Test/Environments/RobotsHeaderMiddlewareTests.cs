using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using Moq;

using NUnit.Framework;

using Stott.Optimizely.RobotsHandler.Environments;

namespace Stott.Optimizely.RobotsHandler.Test.Environments;

[TestFixture]
public sealed class RobotsHeaderMiddlewareTests
{
    private Mock<RequestDelegate> _mockNext;

    private Mock<HttpContext> _mockContext;

    private Mock<HttpResponse> _mockResponse;

    private Mock<ILogger<RobotsHeaderMiddleware>> _mockLogger;

    private RobotsHeaderMiddleware middleware;

    [SetUp]
    public void SetUp()
    {
        _mockNext = new Mock<RequestDelegate>();

        _mockResponse = new Mock<HttpResponse>();
        _mockResponse.Setup(response => response.Headers).Returns(new HeaderDictionary());

        _mockContext = new Mock<HttpContext>();
        _mockContext.SetupGet(context => context.Response).Returns(_mockResponse.Object);

        _mockLogger = new Mock<ILogger<RobotsHeaderMiddleware>>();

        middleware = new RobotsHeaderMiddleware(_mockNext.Object);
    }

    [Test]
    public async Task Invoke_GivenTheServiceReturnsNull_ThenNoHeaderIsAdded()
    {
        // Arrange
        var mockService = new Mock<IEnvironmentRobotsService>();
        mockService.Setup(service => service.GetCurrent()).Returns((EnvironmentRobotsModel)null);

        // Act
        await middleware.Invoke(_mockContext.Object, mockService.Object, _mockLogger.Object);

        // Assert
        Assert.That(_mockResponse.Object.Headers, Is.Empty);
    }

    [Test]
    public async Task Invoke_GivenTheServiceReturnsAConfigurationThatIsDisabled_ThenNoHeaderIsAdded()
    {
        // Arrange
        var mockService = new Mock<IEnvironmentRobotsService>();
        mockService.Setup(service => service.GetCurrent()).Returns(new EnvironmentRobotsModel());

        // Act
        await middleware.Invoke(_mockContext.Object, mockService.Object, _mockLogger.Object);

        // Assert
        Assert.That(_mockResponse.Object.Headers, Is.Empty);
    }

    [Test]
    public async Task Invoke_GivenTheServiceReturnsAConfigurationThatIsEnabled_ThenTheHeaderIsAdded()
    {
        // Arrange
        var mockService = new Mock<IEnvironmentRobotsService>();
        mockService.Setup(service => service.GetCurrent()).Returns(new EnvironmentRobotsModel { UseNoFollow = true });

        // Act
        await middleware.Invoke(_mockContext.Object, mockService.Object, _mockLogger.Object);

        // Assert
        Assert.That(_mockResponse.Object.Headers.ContainsKey("X-Robots-Tag"), Is.True);
    }
}
