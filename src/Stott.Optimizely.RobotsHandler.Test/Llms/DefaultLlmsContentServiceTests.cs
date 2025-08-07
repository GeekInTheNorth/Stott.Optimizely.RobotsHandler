using System;
using System.Collections.Generic;
using System.Text;

using EPiServer.Web;

using Moq;

using NUnit.Framework;

using Stott.Optimizely.RobotsHandler.Llms;
using Stott.Optimizely.RobotsHandler.Models;

namespace Stott.Optimizely.RobotsHandler.Test.Llms;

[TestFixture]
public class DefaultLlmsContentServiceTests
{
    private Mock<HostDefinition> _mockHostDefinition;

    private Mock<SiteDefinition> _mockSiteDefinition;

    private Mock<ISiteDefinitionRepository> _mockSiteDefinitionRepository;

    private Mock<ILlmsContentRepository> _mockRepository;

    private DefaultLlmsContentService _service;

    [SetUp]
    public void SetUp()
    {
        _mockHostDefinition = new Mock<HostDefinition>();

        _mockSiteDefinition = new Mock<SiteDefinition>();
        _mockSiteDefinition.Setup(x => x.Hosts).Returns(new List<HostDefinition> { _mockHostDefinition.Object });

        _mockSiteDefinitionRepository = new Mock<ISiteDefinitionRepository>();
        _mockSiteDefinitionRepository.Setup(x => x.List()).Returns(new List<SiteDefinition> { _mockSiteDefinition.Object });

        _mockRepository = new Mock<ILlmsContentRepository>();

        _service = new DefaultLlmsContentService(_mockSiteDefinitionRepository.Object, _mockRepository.Object);
    }

    [Test]
    public void GetLlmsContent_siteId_ReturnsNullForAValidSiteWhenLlmsContentDoesNotExist()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        _mockRepository.Setup(x => x.Get(It.IsAny<Guid>())).Returns((LlmsTxtEntity)null);
        _mockRepository.Setup(x => x.GetAllForSite(It.IsAny<Guid>())).Returns(new List<LlmsTxtEntity>(0));

        // Act
        var LlmsContent = _service.GetLlmsContent(siteId, null);

        // Assert
        Assert.That(LlmsContent, Is.Null);
    }

    [Test]
    public void GetLlmsContent_siteId_ReturnsLlmsContentForASpecificHostWhenLlmsContentExists()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var LlmsContent = GetSavedLlms();
        var LlmsTxtEntity = new LlmsTxtEntity { Id = Guid.NewGuid(), SiteId = siteId, SpecificHost = "www.example.com", LlmsContent = LlmsContent };
        _mockRepository.Setup(x => x.GetAllForSite(It.IsAny<Guid>())).Returns(new List<LlmsTxtEntity> { LlmsTxtEntity });

        // Act
        var result = _service.GetLlmsContent(siteId, "www.example.com");

        // Assert
        Assert.That(result, Is.EqualTo(LlmsContent));
    }

    [Test]
    public void GetLlmsContent_siteId_ReturnsDefaultRobotsForASiteWhenLlmsContentExistsButNoSpecificHostIsProvided()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var LlmsContent = GetSavedLlms();
        var LlmsTxtEntity = new LlmsTxtEntity { Id = Guid.NewGuid(), SiteId = siteId, SpecificHost = string.Empty, LlmsContent = LlmsContent };
        _mockRepository.Setup(x => x.GetAllForSite(It.IsAny<Guid>())).Returns(new List<LlmsTxtEntity> { LlmsTxtEntity });

        // Act
        var result = _service.GetLlmsContent(siteId, null);

        // Assert
        Assert.That(result, Is.EqualTo(LlmsContent));
    }

    [Test]
    public void GetLlmsContent_siteId_ReturnsNullForASiteWhenLlmsContentExistsButNoMatchingSpecificHostIsProvided()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var LlmsContent = GetSavedLlms();
        var LlmsTxtEntity = new LlmsTxtEntity { Id = Guid.NewGuid(), SiteId = siteId, SpecificHost = "www.example.com", LlmsContent = LlmsContent };
        _mockRepository.Setup(x => x.GetAllForSite(It.IsAny<Guid>())).Returns(new List<LlmsTxtEntity> { LlmsTxtEntity });

        // Act
        var result = _service.GetLlmsContent(siteId, "www.non-matching.com");

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetLlmsContent_siteId_ReturnsMatchingHostWhenBothDefaultAndDefinedHostExist()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var LlmsContent = GetSavedLlms();
        var hostDefinedEntity = new LlmsTxtEntity { Id = Guid.NewGuid(), SiteId = siteId, SpecificHost = "www.example.com", LlmsContent = "Defined Robots" };
        var defaultEntity = new LlmsTxtEntity { Id = Guid.NewGuid(), SiteId = siteId, LlmsContent = "Default Robots" };
        _mockRepository.Setup(x => x.GetAllForSite(It.IsAny<Guid>())).Returns(new List<LlmsTxtEntity> { hostDefinedEntity, defaultEntity });

        // Act
        var result = _service.GetLlmsContent(siteId, "www.example.com");

        // Assert
        Assert.That(result, Is.EqualTo(hostDefinedEntity.LlmsContent));
    }

    [Test]
    public void GetLlmsContent_siteId_ReturnsDefaultHostWhenBothDefaultAndDefinedHostExistForANonMatchingHost()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var LlmsContent = GetSavedLlms();
        var hostDefinedEntity = new LlmsTxtEntity { Id = Guid.NewGuid(), SiteId = siteId, SpecificHost = "www.example.com", LlmsContent = "Defined Robots" };
        var defaultEntity = new LlmsTxtEntity { Id = Guid.NewGuid(), SiteId = siteId, LlmsContent = "Default Robots" };
        _mockRepository.Setup(x => x.GetAllForSite(It.IsAny<Guid>())).Returns(new List<LlmsTxtEntity> { hostDefinedEntity, defaultEntity });

        // Act
        var result = _service.GetLlmsContent(siteId, "www.non-matching.com");

        // Assert
        Assert.That(result, Is.EqualTo(defaultEntity.LlmsContent));
    }

    [Test]
    public void SaveLlmsContent_ThrowsArgumentExceptionWhenPassedAnEmptyGuid()
    {
        // Arrange
        var model = new SaveLlmsModel
        {
            Id = Guid.NewGuid(),
            SiteId = Guid.Empty,
            LlmsContent = GetSavedLlms()
        };

        // Assert
        Assert.Throws<ArgumentException>(() => _service.Save(model));
    }

    [Test]
    public void SaveLlmsContent_ThrowsArgumentExceptionWhenSiteIdRefersToANonExistantSite()
    {
        // Arrange
        var model = new SaveLlmsModel
        {
            Id = Guid.NewGuid(),
            SiteId = Guid.NewGuid(),
            LlmsContent = GetSavedLlms()
        };

        _mockSiteDefinitionRepository.Setup(x => x.Get(It.IsAny<Guid>())).Returns((SiteDefinition)null);

        // Assert
        Assert.Throws<ArgumentException>(() => _service.Save(model));
    }

    [Test]
    public void SaveLlmsContent_CallsSaveOnTheLlmsContentRepositoryForAValidSite()
    {
        // Arrange
        var model = new SaveLlmsModel
        {
            Id = Guid.NewGuid(),
            SiteId = Guid.NewGuid(),
            LlmsContent = GetSavedLlms()
        };

        _mockSiteDefinitionRepository.Setup(x => x.Get(It.IsAny<Guid>())).Returns(_mockSiteDefinition.Object);

        // Act
        _service.Save(model);

        // Assert
        _mockRepository.Verify(x => x.Save(It.IsAny<SaveLlmsModel>()), Times.Once);
    }

    [Test]
    public void Delete_WhenPassedAnEmptyGuid_ThenDeleteIsNotCalledOnTheRepository()
    {
        // Arrange
        var id = Guid.Empty;

        // Act
        _service.Delete(id);

        // Assert
        _mockRepository.Verify(x => x.Delete(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public void Delete_WhenPassedANonEmptyGuid_ThenDeleteIsCalledOnTheRepository()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        _service.Delete(id);

        // Assert
        _mockRepository.Verify(x => x.Delete(It.IsAny<Guid>()), Times.Once);
    }

    [Test]
    public void DoesConflictExists_WhenPassedAnEmptyGuid_ThenFalseIsReturned()
    {
        // Arrange
        var model = new SaveLlmsModel { Id = Guid.Empty };

        // Act
        var result = _service.DoesConflictExists(model);

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
        var savedRecords = new List<LlmsTxtEntity>
        {
            new()
            {
                Id = Guid.Parse("f70719d4-adc6-4a06-8662-7c7e78ab3dbc"),
                SiteId = Guid.Parse("5645bc86-f7f7-4c3a-924c-13612c55914a"),
                SpecificHost = string.Empty,
                LlmsContent = GetSavedLlms()
            },
            new()
            {
                Id = Guid.Parse("a841af98-cdbd-4e64-82b2-f31f3b0fe647"),
                SiteId = Guid.Parse("5645bc86-f7f7-4c3a-924c-13612c55914a"),
                SpecificHost = "www.example.com",
                LlmsContent = GetSavedLlms()
            }
        };

        var model = new SaveLlmsModel { Id = id, SiteId = siteId, SpecificHost = host, LlmsContent = GetSavedLlms() };
        _mockRepository.Setup(x => x.GetAll()).Returns(savedRecords);

        // Act
        var result = _service.DoesConflictExists(model);

        // Assert
        Assert.That(result, Is.EqualTo(expectedValue));
    }

    [Test]
    public void GetDefault_WhenPassedAnInvalidSiteId_ThenThrowsArgumentException()
    {
        // Arrange
        var siteId = Guid.Empty;

        // Assert
        Assert.Throws<ArgumentException>(() => _service.GetDefault(siteId));
    }

    [Test]
    public void GetDefault_WhenPassedAValidSiteId_ButTheRepositoryDoesNotContainTheSite_ThenThrowsArgumentException()
    {
        // Arrange
        var siteId = Guid.NewGuid();

        _mockSiteDefinitionRepository.Setup(x => x.Get(It.IsAny<Guid>())).Returns((SiteDefinition)null);

        // Assert
        Assert.Throws<ArgumentException>(() => _service.GetDefault(siteId));
    }

    [Test]
    public void GetDefault_WhenPassedAValidSiteId_ThenReturnsAValidSiteRobotsViewModel()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var mockSiteDefinition = new SiteDefinition { Id = siteId };

        _mockSiteDefinitionRepository.Setup(x => x.Get(It.IsAny<Guid>())).Returns(mockSiteDefinition);

        // Act
        var result = _service.GetDefault(siteId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(Guid.Empty));
        Assert.That(result.SiteId, Is.EqualTo(siteId));
        Assert.That(result.SpecificHost, Is.Null);
    }

    [Test]
    public void GetAll_WhenTheLlmsContentRepositoryHasNoRecords_ThenAnEmptyCollectionShouldBeReturned()
    {
        // Arrange
        var sites = new List<SiteDefinition>
        {
            new() { Id = Guid.NewGuid() },
            new() { Id = Guid.NewGuid() }
        };

        _mockRepository.Setup(x => x.GetAll()).Returns(new List<LlmsTxtEntity>(0));
        _mockSiteDefinitionRepository.Setup(x => x.List()).Returns(sites);

        // Act
        var result = _service.GetAll();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void GetAll_WhenTheLlmsContentRepositoryHasRecords_ThenAnEntryIsReturnedForEachItem()
    {
        // Arrange
        var sites = new List<SiteDefinition>
        {
            new() { Id = Guid.NewGuid() },
            new() { Id = Guid.NewGuid() }
        };

        var robotsEntities = new List<LlmsTxtEntity>
        {
            new() { Id = Guid.NewGuid(), SiteId = sites[0].Id },
            new() { Id = Guid.NewGuid(), SiteId = sites[1].Id }
        };

        _mockRepository.Setup(x => x.GetAll()).Returns(robotsEntities);
        _mockSiteDefinitionRepository.Setup(x => x.List()).Returns(sites);

        // Act
        var result = _service.GetAll();

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
    public void GetAll_WhenThereAreLlmsContentForSpecificHosts_ThenAnEntryIsReturnedForEachItem()
    {
        // Arrange
        var sites = new List<SiteDefinition>
        {
            new() { Id = Guid.NewGuid(), Name = "Site 1", Hosts = new List<HostDefinition> { new() { Name = "www.exampleone.com" } } },
            new() { Id = Guid.NewGuid(), Name = "Site 2", Hosts = new List<HostDefinition> { new() { Name = "www.exampletwo.com" } } },
        };

        var robotsEntities = new List<LlmsTxtEntity>
        {
            new() { Id = Guid.NewGuid(), SiteId = sites[0].Id, SpecificHost = "www.exampleone.com" },
            new() { Id = Guid.NewGuid(), SiteId = sites[1].Id, SpecificHost = "www.exampletwo.com" }
        };

        _mockRepository.Setup(x => x.GetAll()).Returns(robotsEntities);
        _mockSiteDefinitionRepository.Setup(x => x.List()).Returns(sites);

        // Act
        var result = _service.GetAll();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Has.Count.EqualTo(2));

        Assert.That(result[0].SiteId, Is.EqualTo(sites[0].Id));
        Assert.That(result[0].SiteName, Is.EqualTo(sites[0].Name));
        Assert.That(result[0].SpecificHost, Is.EqualTo("www.exampleone.com"));
        Assert.That(result[0].AvailableHosts[0].DisplayName, Is.EqualTo("Default"));
        Assert.That(result[0].AvailableHosts[0].HostName, Is.EqualTo(string.Empty));
        Assert.That(result[0].AvailableHosts[1].DisplayName, Is.EqualTo("www.exampleone.com"));
        Assert.That(result[0].AvailableHosts[1].HostName, Is.EqualTo("www.exampleone.com"));

        Assert.That(result[1].SiteId, Is.EqualTo(sites[1].Id));
        Assert.That(result[1].SiteName, Is.EqualTo(sites[1].Name));
        Assert.That(result[1].SpecificHost, Is.EqualTo("www.exampletwo.com"));
        Assert.That(result[1].AvailableHosts[0].DisplayName, Is.EqualTo("Default"));
        Assert.That(result[1].AvailableHosts[0].HostName, Is.EqualTo(string.Empty));
        Assert.That(result[1].AvailableHosts[1].DisplayName, Is.EqualTo("www.exampletwo.com"));
        Assert.That(result[1].AvailableHosts[1].HostName, Is.EqualTo("www.exampletwo.com"));
    }

    [Test]
    public void Get_WhenPassedAnIdForALlmsTxtEntityThatDoesNotExist_ThenThrowsLlmsTxtEntityNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepository.Setup(x => x.Get(It.IsAny<Guid>())).Returns((LlmsTxtEntity)null);

        // Assert
        Assert.Throws<RobotsEntityNotFoundException>(() => _service.Get(id));
    }

    [Test]
    public void Get_WhenPassedAnIdForALlmsTxtEntityThatDoesExistButForANonMatchingSite_ThenThrowsLlmsTxtEntityNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var LlmsTxtEntity = new LlmsTxtEntity { Id = id, SiteId = Guid.NewGuid() };
        _mockRepository.Setup(x => x.Get(It.IsAny<Guid>())).Returns(LlmsTxtEntity);

        // Assert
        Assert.Throws<RobotsEntityNotFoundException>(() => _service.Get(id));
    }

    [Test]
    public void Get_WhenPassedAnIdForALlmsTxtEntityThatExists_ThenReturnsTheModel()
    {
        // Arrange
        var id = Guid.NewGuid();
        var siteId = Guid.NewGuid();
        var LlmsTxtEntity = new LlmsTxtEntity { Id = id, SiteId = siteId };
        _mockRepository.Setup(x => x.Get(It.IsAny<Guid>())).Returns(LlmsTxtEntity);

        _mockSiteDefinition.Setup(x => x.Id).Returns(siteId);

        // Act
        var result = _service.Get(id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(id));
    }

    private static string GetSavedLlms()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("User-agent: *");
        stringBuilder.AppendLine("Disallow: /someurl/");
        stringBuilder.AppendLine("# This is a test content item.");

        return stringBuilder.ToString();
    }
}
