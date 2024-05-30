using System;

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

    private RobotsApiController _controller;

    [SetUp]
    public void SetUp()
    {
        _mockService = new Mock<IRobotsContentService>();

        _controller = new RobotsApiController(_mockService.Object);
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