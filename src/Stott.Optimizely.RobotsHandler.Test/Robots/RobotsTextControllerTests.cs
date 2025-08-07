using System;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Moq;

using NUnit.Framework;

using Stott.Optimizely.RobotsHandler.Robots;

namespace Stott.Optimizely.RobotsHandler.Test.Robots;

[TestFixture]
public sealed class RobotsTextControllerTests
{
    private RobotsTextController _controller;

    private Mock<IRobotsContentService> _serviceMock;

    private Mock<HttpRequest> _mockHttpRequest;

    private Mock<HttpResponse> _mockHttpResponse;

    private Mock<HttpContext> _mockHttpContext;

    private Mock<ILogger<RobotsTextController>> _loggerMock;

    [SetUp]
    public void SetUp()
    {
        _serviceMock = new Mock<IRobotsContentService>();
        _loggerMock = new Mock<ILogger<RobotsTextController>>();

        _mockHttpRequest = new Mock<HttpRequest>();
        _mockHttpRequest.Setup(x => x.Host).Returns(new HostString("www.example.com"));

        _mockHttpResponse = new Mock<HttpResponse>();

        _mockHttpContext = new Mock<HttpContext>();
        _mockHttpContext.Setup(x => x.Request).Returns(_mockHttpRequest.Object);
        _mockHttpContext.Setup(x => x.Response).Returns(_mockHttpResponse.Object);

        _controller = new RobotsTextController(_serviceMock.Object, _loggerMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = _mockHttpContext.Object
            }
        };
    }

    [Test]
    public void Index_WhenCalled_ReturnsRobotsContent()
    {
        // Arrange
        var robotsContent = "User-agent: *\nDisallow: /";
        _serviceMock.Setup(x => x.GetRobotsContent(It.IsAny<Guid>(), It.IsAny<string>())).Returns(robotsContent);

        // Act
        var result = _controller.Index();

        // Assert
        var contentResult = result as ContentResult;
        Assert.That(contentResult, Is.Not.Null);
        Assert.That(contentResult.Content, Is.EqualTo(robotsContent));
        Assert.That(contentResult.ContentType, Is.EqualTo("text/plain"));
        Assert.That(contentResult.StatusCode, Is.EqualTo(200));
    }
}