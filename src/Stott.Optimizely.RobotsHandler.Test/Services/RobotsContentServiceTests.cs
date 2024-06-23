using System;
using System.Collections.Generic;
using System.Text;

using EPiServer.LinkAnalyzer.Robots.Internal;
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
        _mockRobotsContentRepository.Setup(x => x.GetAllForSite(It.IsAny<Guid>())).Returns(new List<RobotsEntity>(0));

        // Act
        var robotsContent = _robotsContentService.GetRobotsContent(siteId, null);

        // Assert
        Assert.That(robotsContent, Is.EqualTo(_robotsContentService.GetDefaultRobotsContent()));
    }

    [Test]
    public void GetRobotsContent_siteId_ReturnsRobotsContentForASpecificHostWhenRobotsContentExists()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var robotsContent = GetSavedRobots();
        var robotsEntity = new RobotsEntity { Id = Guid.NewGuid(), SiteId = siteId, SpecificHost = "www.example.com", RobotsContent = robotsContent };
        _mockRobotsContentRepository.Setup(x => x.GetAllForSite(It.IsAny<Guid>())).Returns(new List<RobotsEntity> { robotsEntity });

        // Act
        var result = _robotsContentService.GetRobotsContent(siteId, "www.example.com");

        // Assert
        Assert.That(result, Is.EqualTo(robotsContent));
    }

    [Test]
    public void GetRobotsContent_siteId_ReturnsDefaultRobotsForASiteWhenRobotsContentExistsButNoSpecificHostIsProvided()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var robotsContent = GetSavedRobots();
        var robotsEntity = new RobotsEntity { Id = Guid.NewGuid(), SiteId = siteId, SpecificHost = string.Empty, RobotsContent = robotsContent };
        _mockRobotsContentRepository.Setup(x => x.GetAllForSite(It.IsAny<Guid>())).Returns(new List<RobotsEntity> { robotsEntity });

        // Act
        var result = _robotsContentService.GetRobotsContent(siteId, null);

        // Assert
        Assert.That(result, Is.EqualTo(robotsContent));
    }

    [Test]
    public void GetRobotsContent_siteId_ReturnsDefaultRobotsForASiteWhenRobotsContentExistsButNoMatchingSpecificHostIsProvided()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var robotsContent = GetSavedRobots();
        var robotsEntity = new RobotsEntity { Id = Guid.NewGuid(), SiteId = siteId, SpecificHost = "www.example.com", RobotsContent = robotsContent };
        _mockRobotsContentRepository.Setup(x => x.GetAllForSite(It.IsAny<Guid>())).Returns(new List<RobotsEntity> { robotsEntity });

        // Act
        var result = _robotsContentService.GetRobotsContent(siteId, "www.non-matching.com");

        // Assert
        Assert.That(result, Is.EqualTo(_robotsContentService.GetDefaultRobotsContent()));
    }

    [Test]
    public void GetRobotsContent_siteId_ReturnsMatchingHostWhenBothDefaultAndDefinedHostExist()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var robotsContent = GetSavedRobots();
        var hostDefinedEntity = new RobotsEntity { Id = Guid.NewGuid(), SiteId = siteId, SpecificHost = "www.example.com", RobotsContent = "Defined Robots" };
        var defaultEntity = new RobotsEntity { Id = Guid.NewGuid(), SiteId = siteId, RobotsContent = "Default Robots" };
        _mockRobotsContentRepository.Setup(x => x.GetAllForSite(It.IsAny<Guid>())).Returns(new List<RobotsEntity> { hostDefinedEntity, defaultEntity });

        // Act
        var result = _robotsContentService.GetRobotsContent(siteId, "www.example.com");

        // Assert
        Assert.That(result, Is.EqualTo(hostDefinedEntity.RobotsContent));
    }

    [Test]
    public void GetRobotsContent_siteId_ReturnsDefaultHostWhenBothDefaultAndDefinedHostExistForANonMatchingHost()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var robotsContent = GetSavedRobots();
        var hostDefinedEntity = new RobotsEntity { Id = Guid.NewGuid(), SiteId = siteId, SpecificHost = "www.example.com", RobotsContent = "Defined Robots" };
        var defaultEntity = new RobotsEntity { Id = Guid.NewGuid(), SiteId = siteId, RobotsContent = "Default Robots" };
        _mockRobotsContentRepository.Setup(x => x.GetAllForSite(It.IsAny<Guid>())).Returns(new List<RobotsEntity> { hostDefinedEntity, defaultEntity });

        // Act
        var result = _robotsContentService.GetRobotsContent(siteId, "www.non-matching.com");

        // Assert
        Assert.That(result, Is.EqualTo(defaultEntity.RobotsContent));
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
    [TestCase("a841af98-cdbd-4e64-82b2-f31f3b0fe647", "5645bc86-f7f7-4c3a-924c-13612c55914a", "", true)]
    [TestCase("db107c5e-73ff-442f-93f3-bd99f56603f5", "5645bc86-f7f7-4c3a-924c-13612c55914a", "", true)]
    [TestCase("00000000-0000-0000-0000-000000000000", "5645bc86-f7f7-4c3a-924c-13612c55914a", "", true)]
    [TestCase("a841af98-cdbd-4e64-82b2-f31f3b0fe647", "5645bc86-f7f7-4c3a-924c-13612c55914a", "www.example.com", false)]
    [TestCase("db107c5e-73ff-442f-93f3-bd99f56603f5", "5645bc86-f7f7-4c3a-924c-13612c55914a", "www.example.com", true)]
    [TestCase("00000000-0000-0000-0000-000000000000", "5645bc86-f7f7-4c3a-924c-13612c55914a", "www.example.com", true)]
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
            new() { Id = Guid.NewGuid() },
            new() { Id = Guid.NewGuid() }
        };

        _mockRobotsContentRepository.Setup(x => x.GetAll()).Returns(new List<RobotsEntity>(0));
        _mockSiteDefinitionRepository.Setup(x => x.List()).Returns(sites);

        // Act
        var result = _robotsContentService.GetAll();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result[0].SiteId, Is.EqualTo(sites[0].Id));
        Assert.That(result[0].AvailableHosts[0].DisplayName, Is.EqualTo("Default"));
        Assert.That(result[0].AvailableHosts[0].HostName, Is.EqualTo(string.Empty));
        Assert.That(result[1].SiteId, Is.EqualTo(sites[1].Id));
        Assert.That(result[1].AvailableHosts[0].DisplayName, Is.EqualTo("Default"));
        Assert.That(result[1].AvailableHosts[0].HostName, Is.EqualTo(string.Empty));
    }

    [Test]
    public void GetAll_WhenTheRobotsContentRepositoryHasRecords_ThenTheRecordsShouldBeReturnedForEachSite()
    {
        // Arrange
        var sites = new List<SiteDefinition>
        {
            new() { Id = Guid.NewGuid() },
            new() { Id = Guid.NewGuid() }
        };

        var robotsEntities = new List<RobotsEntity>
        {
            new() { Id = Guid.NewGuid(), SiteId = sites[0].Id },
            new() { Id = Guid.NewGuid(), SiteId = sites[1].Id }
        };

        _mockRobotsContentRepository.Setup(x => x.GetAll()).Returns(robotsEntities);
        _mockSiteDefinitionRepository.Setup(x => x.List()).Returns(sites);

        // Act
        var result = _robotsContentService.GetAll();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result[0].SiteId, Is.EqualTo(sites[0].Id));
        Assert.That(result[0].AvailableHosts[0].DisplayName, Is.EqualTo("Default"));
        Assert.That(result[0].AvailableHosts[0].HostName, Is.EqualTo(string.Empty));
        Assert.That(result[1].SiteId, Is.EqualTo(sites[1].Id));
        Assert.That(result[1].AvailableHosts[0].DisplayName, Is.EqualTo("Default"));
        Assert.That(result[1].AvailableHosts[0].HostName, Is.EqualTo(string.Empty));
    }

    [Test]
    public void GetAll_WhenThereAreRobotsContentForSpecificHosts_ThenTheSpecificHostsShouldBeReturned()
    {
        // Arrange
        var sites = new List<SiteDefinition>
        {
            new() { Id = Guid.NewGuid(), Name = "Site 1", Hosts = new List<HostDefinition> { new() { Name = "www.exampleone.com" } } },
            new() { Id = Guid.NewGuid(), Name = "Site 2", Hosts = new List<HostDefinition> { new() { Name = "www.exampletwo.com" } } },
        };

        var robotsEntities = new List<RobotsEntity>
        {
            new() { Id = Guid.NewGuid(), SiteId = sites[0].Id, SpecificHost = "www.exampleone.com" },
            new() { Id = Guid.NewGuid(), SiteId = sites[1].Id, SpecificHost = "www.exampletwo.com" }
        };

        _mockRobotsContentRepository.Setup(x => x.GetAll()).Returns(robotsEntities);
        _mockSiteDefinitionRepository.Setup(x => x.List()).Returns(sites);

        // Act
        var result = _robotsContentService.GetAll();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Has.Count.EqualTo(4));
        
        Assert.That(result[0].SiteId, Is.EqualTo(sites[0].Id));
        Assert.That(result[0].SiteName, Is.EqualTo(sites[0].Name));
        Assert.That(result[0].SpecificHost, Is.Null);
        Assert.That(result[0].CanDelete, Is.False);
        Assert.That(result[0].AvailableHosts[0].DisplayName, Is.EqualTo("Default"));
        Assert.That(result[0].AvailableHosts[0].HostName, Is.EqualTo(string.Empty));
        Assert.That(result[0].AvailableHosts[1].DisplayName, Is.EqualTo("www.exampleone.com"));
        Assert.That(result[0].AvailableHosts[1].HostName, Is.EqualTo("www.exampleone.com"));

        Assert.That(result[1].SiteId, Is.EqualTo(sites[0].Id));
        Assert.That(result[1].SiteName, Is.EqualTo(sites[0].Name));
        Assert.That(result[1].SpecificHost, Is.EqualTo("www.exampleone.com"));
        Assert.That(result[1].CanDelete, Is.True);
        Assert.That(result[1].AvailableHosts[0].DisplayName, Is.EqualTo("Default"));
        Assert.That(result[1].AvailableHosts[0].HostName, Is.EqualTo(string.Empty));
        Assert.That(result[1].AvailableHosts[1].DisplayName, Is.EqualTo("www.exampleone.com"));
        Assert.That(result[1].AvailableHosts[1].HostName, Is.EqualTo("www.exampleone.com"));

        Assert.That(result[2].SiteId, Is.EqualTo(sites[1].Id));
        Assert.That(result[2].SiteName, Is.EqualTo(sites[1].Name));
        Assert.That(result[2].SpecificHost, Is.Null);
        Assert.That(result[2].CanDelete, Is.False);
        Assert.That(result[2].AvailableHosts[0].DisplayName, Is.EqualTo("Default"));
        Assert.That(result[2].AvailableHosts[0].HostName, Is.EqualTo(string.Empty));
        Assert.That(result[2].AvailableHosts[1].DisplayName, Is.EqualTo("www.exampletwo.com"));
        Assert.That(result[2].AvailableHosts[1].HostName, Is.EqualTo("www.exampletwo.com"));

        Assert.That(result[3].SiteId, Is.EqualTo(sites[1].Id));
        Assert.That(result[3].SiteName, Is.EqualTo(sites[1].Name));
        Assert.That(result[3].SpecificHost, Is.EqualTo("www.exampletwo.com"));
        Assert.That(result[3].CanDelete, Is.True);
        Assert.That(result[3].AvailableHosts[0].DisplayName, Is.EqualTo("Default"));
        Assert.That(result[3].AvailableHosts[0].HostName, Is.EqualTo(string.Empty));
        Assert.That(result[3].AvailableHosts[1].DisplayName, Is.EqualTo("www.exampletwo.com"));
        Assert.That(result[3].AvailableHosts[1].HostName, Is.EqualTo("www.exampletwo.com"));
    }

    [Test]
    public void Get_WhenPassedAnIdForARobotsEntityThatDoesNotExist_ThenThrowsRobotsEntityNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRobotsContentRepository.Setup(x => x.Get(It.IsAny<Guid>())).Returns((RobotsEntity)null);

        // Assert
        Assert.Throws<RobotsEntityNotFoundException>(() => _robotsContentService.Get(id));
    }

    [Test]
    public void Get_WhenPassedAnIdForARobotsEntityThatDoesExistButForANonMatchingSite_ThenThrowsRobotsEntityNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var robotsEntity = new RobotsEntity { Id = id, SiteId = Guid.NewGuid() };
        _mockRobotsContentRepository.Setup(x => x.Get(It.IsAny<Guid>())).Returns(robotsEntity);

        // Assert
        Assert.Throws<RobotsEntityNotFoundException>(() => _robotsContentService.Get(id));
    }

    [Test]
    public void Get_WhenPassedAnIdForARobotsEntityThatExists_ThenReturnsTheModel()
    {
        // Arrange
        var id = Guid.NewGuid();
        var siteId = Guid.NewGuid();
        var robotsEntity = new RobotsEntity { Id = id, SiteId = siteId };
        _mockRobotsContentRepository.Setup(x => x.Get(It.IsAny<Guid>())).Returns(robotsEntity);

        _mockSiteDefinition.Setup(x => x.Id).Returns(siteId);

        // Act
        var result = _robotsContentService.Get(id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(id));
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