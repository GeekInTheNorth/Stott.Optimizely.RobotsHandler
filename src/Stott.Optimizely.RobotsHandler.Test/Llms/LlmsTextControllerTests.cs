using System.Threading.Tasks;

using EPiServer.Applications;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Moq;

using NUnit.Framework;

using Stott.Optimizely.RobotsHandler.Llms;

namespace Stott.Optimizely.RobotsHandler.Test.Llms;

[TestFixture]
public class LlmsTextControllerTests
{
    private LlmsTextController _controller;

    private Mock<IApplicationResolver> _mockApplicationResolver;

    private Mock<ILlmsContentService> _serviceMock;

    private Mock<HttpRequest> _mockHttpRequest;

    private Mock<HttpResponse> _mockHttpResponse;

    private Mock<HttpContext> _mockHttpContext;

    private Mock<ILogger<LlmsTextController>> _loggerMock;

    [SetUp]
    public void SetUp()
    {
        _mockApplicationResolver = new Mock<IApplicationResolver>();
        _serviceMock = new Mock<ILlmsContentService>();
        _loggerMock = new Mock<ILogger<LlmsTextController>>();

        _mockHttpRequest = new Mock<HttpRequest>();
        _mockHttpRequest.Setup(x => x.Host).Returns(new HostString("www.example.com"));

        _mockHttpResponse = new Mock<HttpResponse>();
        _mockHttpResponse.Setup(x => x.Headers).Returns(new HeaderDictionary());

        _mockHttpContext = new Mock<HttpContext>();
        _mockHttpContext.Setup(x => x.Request).Returns(_mockHttpRequest.Object);
        _mockHttpContext.Setup(x => x.Response).Returns(_mockHttpResponse.Object);

        _controller = new LlmsTextController(_mockApplicationResolver.Object, _serviceMock.Object, _loggerMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = _mockHttpContext.Object
            }
        };
    }

    [Test]
    public async Task Index_WhenCalled_ReturnsRobotsContent()
    {
        // Arrange
        var robotsContent = "User-agent: *\nDisallow: /";
        _serviceMock.Setup(x => x.GetLlmsContent(It.IsAny<string>(), It.IsAny<string>())).Returns(robotsContent);

        // Act
        var result = await _controller.Index();

        // Assert
        var contentResult = result as ContentResult;
        Assert.That(contentResult, Is.Not.Null);
        Assert.That(contentResult.Content, Is.EqualTo(robotsContent));
        Assert.That(contentResult.ContentType, Is.EqualTo("text/plain; charset=utf-8"));
        Assert.That(contentResult.StatusCode, Is.EqualTo(200));
    }
}
