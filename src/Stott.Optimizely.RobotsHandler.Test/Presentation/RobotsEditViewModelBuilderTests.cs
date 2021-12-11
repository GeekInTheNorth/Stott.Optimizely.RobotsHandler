using System;

using EPiServer.Web;

using Moq;

using NUnit.Framework;

using Stott.Optimizely.RobotsHandler.Exceptions;
using Stott.Optimizely.RobotsHandler.Presentation;
using Stott.Optimizely.RobotsHandler.Services;

namespace Stott.Optimizely.RobotsHandler.Test.UI
{
    [TestFixture]
    public class RobotsEditViewModelBuilderTests
    {
        private Mock<SiteDefinition> _mockSiteDefinition;

        private Mock<ISiteDefinitionRepository> _mockSiteDefinitionRepository;

        private Mock<IRobotsContentService> _mockRobotsContentService;

        private RobotsEditViewModelBuilder _viewModelBuilder;

        [SetUp]
        public void SetUp()
        {
            _mockSiteDefinition = new Mock<SiteDefinition>();

            _mockSiteDefinitionRepository = new Mock<ISiteDefinitionRepository>();
            _mockRobotsContentService = new Mock<IRobotsContentService>();

            _viewModelBuilder = new RobotsEditViewModelBuilder(_mockSiteDefinitionRepository.Object, _mockRobotsContentService.Object);
        }
        
        [Test]
        public void Build_ThrowsRobotsInvalidSiteIdExceptionWhenSiteIdIsNotProvided()
        {
            // Assert
            Assert.Throws<RobotsInvalidSiteIdException>(() => _viewModelBuilder.Build());
        }

        [Test]
        public void Build_ThrowsRobotsInvalidSiteIdExceptionWhenAnEmptySiteIdIsProvided()
        {
            // Assert
            Assert.Throws<RobotsInvalidSiteIdException>(() => _viewModelBuilder.WithSiteId(Guid.Empty).Build());
        }

        [Test]
        public void Build_ThrowsRobotsInvalidSiteExceptionWhenSiteIdRefersToANonExistantSite()
        {
            // Arrange
            _mockSiteDefinitionRepository.Setup(x => x.Get(It.IsAny<Guid>())).Returns((SiteDefinition)null);

            // Assert
            Assert.Throws<RobotsInvalidSiteException>(() => _viewModelBuilder.WithSiteId(Guid.NewGuid()).Build());
        }

        [Test]
        public void Build_ReturnsAValidModelWhenSiteIdRefersToAValidSiteAndSaveContent()
        {
            // Arrange
            var siteId = Guid.NewGuid();
            var siteName = "My Site";
            var robotsContent = "Some Robots Content";

            _mockSiteDefinition.Setup(x => x.Id).Returns(siteId);
            _mockSiteDefinition.Setup(x => x.Name).Returns(siteName);
            _mockSiteDefinitionRepository.Setup(x => x.Get(It.IsAny<Guid>())).Returns(_mockSiteDefinition.Object);

            _mockRobotsContentService.Setup(x => x.GetRobotsContent(It.IsAny<Guid>())).Returns(robotsContent);

            // Act
            var model = _viewModelBuilder.WithSiteId(siteId).Build();

            // Arrange
            Assert.That(model, Is.Not.Null);
            Assert.That(model.SiteId, Is.EqualTo(siteId));
            Assert.That(model.SiteName, Is.EqualTo(siteName));
            Assert.That(model.RobotsContent, Is.EqualTo(robotsContent));
        }
    }
}
