using System;

using EPiServer.Web;

using Microsoft.Extensions.Logging;

using Moq;

using NUnit.Framework;

using Stott.Optimizely.RobotsHandler.Presentation;
using Stott.Optimizely.RobotsHandler.Services;
using Stott.Optimizely.RobotsHandler.Test.TestCases;

namespace Stott.Optimizely.RobotsHandler.Test.Presentation;

[TestFixture]
public sealed class RobotsApiControllerTests
{
    private Mock<IRobotsContentService> _mockService;

    private Mock<ISiteDefinitionRepository> _mockSiteRepository;

    private Mock<ILogger<RobotsApiController>> _mockLogger;

    private RobotsApiController _controller;

    [SetUp]
    public void SetUp()
    {
        _mockService = new Mock<IRobotsContentService>();

        _mockSiteRepository = new Mock<ISiteDefinitionRepository>();

        _mockLogger = new Mock<ILogger<RobotsApiController>>();

        _controller = new RobotsApiController(_mockService.Object, _mockSiteRepository.Object, _mockLogger.Object);
    }

    [Test]
    [TestCaseSource(typeof(CommonTestCases), nameof(CommonTestCases.InvalidGuidStrings))]
    public void Details_ThrowsArgumentExceptionWhenPresentedWithAnInvalidSiteId(string siteId)
    {
        // Assert
        Assert.Throws<ArgumentException>(() => _controller.Details(Guid.NewGuid().ToString(), siteId));
    }

    [Test]
    public void Details_RetrievesModelWhenPresentedWithAValidSiteId()
    {
        // Arrange
        var siteId = Guid.NewGuid().ToString();

        // Act
        _controller.Details(Guid.NewGuid().ToString(), siteId);

        // Assert
        _mockService.Verify(x => x.Get(It.IsAny<Guid>()), Times.Once);
    }
}