using System;
using System.Collections.Generic;
using System.Text;

using EPiServer.Web;

using Moq;

using NUnit.Framework;

using Stott.Optimizely.RobotsHandler.Models;
using Stott.Optimizely.RobotsHandler.Presentation.ViewModels;
using Stott.Optimizely.RobotsHandler.Services;

namespace Stott.Optimizely.RobotsHandler.Test.Services;

[TestFixture]
public sealed class RobotsContentServiceTests
{
    private Mock<HostDefinition> _mockHostDefinition;

    private Mock<SiteDefinition> _mockSiteDefinition;

    private Mock<ISiteDefinitionRepository> _mockSiteDefinitionRepository;

    private Mock<IRobotsContentRepository> _mockRobotsContentRepository;

    private RobotsContentService _robotsContentService;

    [SetUp]
    public void SetUp()
    {
        _mockHostDefinition = new Mock<HostDefinition>();

        _mockSiteDefinition = new Mock<SiteDefinition>();
        _mockSiteDefinition.Setup(x => x.Hosts).Returns(new List<HostDefinition> { _mockHostDefinition.Object });

        _mockSiteDefinitionRepository = new Mock<ISiteDefinitionRepository>();
        _mockSiteDefinitionRepository.Setup(x => x.List()).Returns(new List<SiteDefinition> { _mockSiteDefinition.Object });

        _mockRobotsContentRepository = new Mock<IRobotsContentRepository>();

        _robotsContentService = new RobotsContentService(_mockSiteDefinitionRepository.Object, _mockRobotsContentRepository.Object);
    }

    [Test]
    public void GetRobotsContent_siteId_ReturnsDefaultRobotsForAValidSiteWhenRobotsContentDoesNotExist()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        _mockRobotsContentRepository.Setup(x => x.Get(It.IsAny<Guid>())).Returns((RobotsEntity)null);

        // Act
        var robotsContent = _robotsContentService.GetRobotsContent(siteId);

        // Assert
        Assert.That(robotsContent, Is.EqualTo(_robotsContentService.GetDefaultRobotsContent()));
    }

    [Test]
    public void GetRobotsContent_siteId_ReturnsSavedRobotsForAValidSiteWhenRobotsContentDoesExist()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var robotsEntity = new RobotsEntity { RobotsContent = GetSavedRobots() };
        _mockRobotsContentRepository.Setup(x => x.Get(It.IsAny<Guid>())).Returns(robotsEntity);

        // Act
        var robotsContent = _robotsContentService.GetRobotsContent(siteId);

        // Assert
        Assert.That(robotsContent, Is.Not.Null);
        Assert.That(robotsContent, Is.Not.EqualTo(_robotsContentService.GetDefaultRobotsContent()));
    }

    [Test]
    public void SaveRobotsContent_ThrowsArgumentExceptionWhenPassedAnEmptyGuid()
    {
        // Arrange
        var model = new SaveRobotsModel
        {
            Id = Guid.NewGuid(),
            SiteId = Guid.Empty,
            RobotsContent = GetSavedRobots()
        };

        // Assert
        Assert.Throws<ArgumentException>(() => _robotsContentService.Save(model));
    }

    [Test]
    public void SaveRobotsContent_ThrowsArgumentExceptionWhenSiteIdRefersToANonExistantSite()
    {
        // Arrange
        var model = new SaveRobotsModel
        {
            Id = Guid.NewGuid(),
            SiteId = Guid.NewGuid(),
            RobotsContent = GetSavedRobots()
        };

        _mockSiteDefinitionRepository.Setup(x => x.Get(It.IsAny<Guid>())).Returns((SiteDefinition)null);

        // Assert
        Assert.Throws<ArgumentException>(() => _robotsContentService.Save(model));
    }

    [Test]
    public void SaveRobotsContent_CallsSaveOnTheRobotsContentRepositoryForAValidSite()
    {
        // Arrange
        var model = new SaveRobotsModel 
        { 
            Id = Guid.NewGuid(),
            SiteId = Guid.NewGuid(), 
            RobotsContent = GetSavedRobots()
        };

        _mockSiteDefinitionRepository.Setup(x => x.Get(It.IsAny<Guid>())).Returns(_mockSiteDefinition.Object);

        // Act
        _robotsContentService.Save(model);

        // Assert
        _mockRobotsContentRepository.Verify(x => x.Save(It.IsAny<SaveRobotsModel>()), Times.Once);
    }

    private static string GetSavedRobots()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("User-agent: *");
        stringBuilder.AppendLine("Disallow: /someurl/");
        stringBuilder.AppendLine("# This is a test content item.");

        return stringBuilder.ToString();
    }
}