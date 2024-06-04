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

    [Test]
    public void Delete_WhenPassedAnEmptyGuid_ThenDeleteIsNotCalledOnTheRepository()
    {
        // Arrange
        var id = Guid.Empty;

        // Act
        _robotsContentService.Delete(id);

        // Assert
        _mockRobotsContentRepository.Verify(x => x.Delete(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public void Delete_WhenPassedANonEmptyGuid_ThenDeleteIsCalledOnTheRepository()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        _robotsContentService.Delete(id);

        // Assert
        _mockRobotsContentRepository.Verify(x => x.Delete(It.IsAny<Guid>()), Times.Once);
    }

    [Test]
    public void DoesConflictExists_WhenPassedAnEmptyGuid_ThenFalseIsReturned()
    {
        // Arrange
        var model = new SaveRobotsModel { Id = Guid.Empty };

        // Act
        var result = _robotsContentService.DoesConflictExists(model);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    [TestCase("f70719d4-adc6-4a06-8662-7c7e78ab3dbc", "5645bc86-f7f7-4c3a-924c-13612c55914a", "", false)]
    [TestCase("db107c5e-73ff-442f-93f3-bd99f56603f5", "5645bc86-f7f7-4c3a-924c-13612c55914a", "", true)]
    [TestCase("a841af98-cdbd-4e64-82b2-f31f3b0fe647", "5645bc86-f7f7-4c3a-924c-13612c55914a", "www.example.com", false)]
    [TestCase("db107c5e-73ff-442f-93f3-bd99f56603f5", "5645bc86-f7f7-4c3a-924c-13612c55914a", "www.example.com", true)]
    [TestCase("db107c5e-73ff-442f-93f3-bd99f56603f5", "5645bc86-f7f7-4c3a-924c-13612c55914a", "www.non-matching.com", false)]
    public void DoesConflictExists_GivenTheRepositoryContainsAConflictingConfiguration_ThenTrueIsReturned(Guid id, Guid siteId, string host, bool expectedValue)
    {
        // Arrange
        var savedRecords = new List<RobotsEntity>
        {
            new()
            { 
                Id = Guid.Parse("f70719d4-adc6-4a06-8662-7c7e78ab3dbc"), 
                SiteId = Guid.Parse("5645bc86-f7f7-4c3a-924c-13612c55914a"), 
                SpecificHost = string.Empty, 
                RobotsContent = GetSavedRobots()
            },
            new()
            {
                Id = Guid.Parse("a841af98-cdbd-4e64-82b2-f31f3b0fe647"),
                SiteId = Guid.Parse("5645bc86-f7f7-4c3a-924c-13612c55914a"),
                SpecificHost = "www.example.com",
                RobotsContent = GetSavedRobots()
            }
        };

        var model = new SaveRobotsModel { Id = id, SiteId = siteId, SpecificHost = host, RobotsContent = GetSavedRobots() };
        _mockRobotsContentRepository.Setup(x => x.GetAll()).Returns(savedRecords);

        // Act
        var result = _robotsContentService.DoesConflictExists(model);

        // Assert
        Assert.That(result, Is.EqualTo(expectedValue));
    }

    [Test]
    public void GetDefault_WhenPassedAnInvalidSiteId_ThenThrowsArgumentException()
    {
        // Arrange
        var siteId = Guid.Empty;

        // Assert
        Assert.Throws<ArgumentException>(() => _robotsContentService.GetDefault(siteId));
    }

    [Test]
    public void GetDefault_WhenPassedAValidSiteId_ButTheRepositoryDoesNotContainTheSite_ThenThrowsArgumentException()
    {
        // Arrange
        var siteId = Guid.NewGuid();

        _mockSiteDefinitionRepository.Setup(x => x.Get(It.IsAny<Guid>())).Returns((SiteDefinition)null);

        // Assert
        Assert.Throws<ArgumentException>(() => _robotsContentService.GetDefault(siteId));
    }

    [Test]
    public void GetDefault_WhenPassedAValidSiteId_ThenReturnsAValidSiteRobotsViewModel()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var mockSiteDefinition = new SiteDefinition { Id = siteId };

        _mockSiteDefinitionRepository.Setup(x => x.Get(It.IsAny<Guid>())).Returns(mockSiteDefinition);

        // Act
        var result = _robotsContentService.GetDefault(siteId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(Guid.Empty));
        Assert.That(result.SiteId, Is.EqualTo(siteId));
        Assert.That(result.SpecificHost, Is.Null);
    }

    [Test]
    public void GetAll_WhenTheRobotsContentRepositoryHasNoRecords_ThenDefaultRecordsShouldBeReturnedForEachSite()
    {
        // Arrange
        var sites = new List<SiteDefinition>
        {
            new SiteDefinition { Id = Guid.NewGuid() },
            new SiteDefinition { Id = Guid.NewGuid() }
        };

        _mockRobotsContentRepository.Setup(x => x.GetAll()).Returns(new List<RobotsEntity>(0));
        _mockSiteDefinitionRepository.Setup(x => x.List()).Returns(sites);

        // Act
        var result = _robotsContentService.GetAll();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result[0].SiteId, Is.EqualTo(sites[0].Id));
        Assert.That(result[1].SiteId, Is.EqualTo(sites[1].Id));
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