using System;

using Moq;

using NUnit.Framework;

using Stott.Optimizely.RobotsHandler.Presentation;
using Stott.Optimizely.RobotsHandler.Services;
using Stott.Optimizely.RobotsHandler.Test.TestCases;

namespace Stott.Optimizely.RobotsHandler.Test.Presentation
{
    [TestFixture]
    public class RobotsControllerTests
    {
        private Mock<IRobotsContentService> _mockService;

        private Mock<IRobotsEditViewModelBuilder> _mockEditViewModelBuilder;

        private Mock<IRobotsListViewModelBuilder> _mockListingViewModelBuilder;

        private RobotsController _controller;

        [SetUp]
        public void SetUp()
        {
            _mockService = new Mock<IRobotsContentService>();

            _mockEditViewModelBuilder = new Mock<IRobotsEditViewModelBuilder>();
            _mockEditViewModelBuilder.Setup(x => x.WithSiteId(It.IsAny<Guid>())).Returns(_mockEditViewModelBuilder.Object);

            _mockListingViewModelBuilder = new Mock<IRobotsListViewModelBuilder>();

            _controller = new RobotsController(_mockService.Object, _mockEditViewModelBuilder.Object, _mockListingViewModelBuilder.Object);
        }

        [Test]
        [TestCaseSource(typeof(CommonTestCases), nameof(CommonTestCases.InvalidGuidStrings))]
        public void Details_ThrowsArgumentExceptionWhenPresentedWithAnInvalidSiteId(string siteId)
        {
            // Assert
            Assert.Throws<ArgumentException>(() => _controller.Details(siteId));
        }

        [Test]
        public void Details_RetrievesModelWhenPresentedWithAValidSiteId()
        {
            // Arrange
            var siteId = Guid.NewGuid().ToString();

            // Act
            _controller.Details(siteId);

            // Assert
            _mockEditViewModelBuilder.Verify(x => x.WithSiteId(It.IsAny<Guid>()), Times.Once);
        }
    }
}
