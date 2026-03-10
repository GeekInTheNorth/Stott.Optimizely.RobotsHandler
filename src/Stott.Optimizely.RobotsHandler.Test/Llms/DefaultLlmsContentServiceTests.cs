using System;
using System.Collections.Generic;
using System.Text;

using Moq;

using NUnit.Framework;
using Stott.Optimizely.RobotsHandler.Applications;
using Stott.Optimizely.RobotsHandler.Llms;
using Stott.Optimizely.RobotsHandler.Models;

namespace Stott.Optimizely.RobotsHandler.Test.Llms;

[TestFixture]
public class DefaultLlmsContentServiceTests
{
    private Mock<IApplicationDefinitionService> _mockApplicationDefinitionRepository;

    private Mock<ILlmsContentRepository> _mockRepository;

    private DefaultLlmsContentService _service;

    [SetUp]
    public void SetUp()
    {
        _mockApplicationDefinitionRepository = new Mock<IApplicationDefinitionService>();
        _mockApplicationDefinitionRepository.Setup(x => x.GetAllApplicationsAsync()).ReturnsAsync([]);

        _mockRepository = new Mock<ILlmsContentRepository>();

        _service = new DefaultLlmsContentService(_mockApplicationDefinitionRepository.Object, _mockRepository.Object);
    }

    [Test]
    public void GetLlmsContent_siteId_ReturnsNullForAValidSiteWhenLlmsContentDoesNotExist()
    {
        // Arrange
        var appId = "appId";
        _mockRepository.Setup(x => x.Get(It.IsAny<Guid>())).Returns((LlmsTxtEntity)null);
        _mockRepository.Setup(x => x.GetAllForSite(It.IsAny<string>())).Returns([]);

        // Act
        var LlmsContent = _service.GetLlmsContent(appId, null);

        // Assert
        Assert.That(LlmsContent, Is.Null);
    }

    [Test]
    public void GetLlmsContent_siteId_ReturnsLlmsContentForASpecificHostWhenLlmsContentExists()
    {
        // Arrange
        var appId = "appId";
        var LlmsContent = GetSavedLlms();
        var LlmsTxtEntity = new LlmsTxtEntity { AppId = appId, SpecificHost = "www.example.com", LlmsContent = LlmsContent };
        _mockRepository.Setup(x => x.GetAllForSite(It.IsAny<string>())).Returns([LlmsTxtEntity]);

        // Act
        var result = _service.GetLlmsContent(appId, "www.example.com");

        // Assert
        Assert.That(result, Is.EqualTo(LlmsContent));
    }

    [Test]
    public void GetLlmsContent_siteId_ReturnsDefaultRobotsForASiteWhenLlmsContentExistsButNoSpecificHostIsProvided()
    {
        // Arrange
        var appId = "appId";
        var LlmsContent = GetSavedLlms();
        var LlmsTxtEntity = new LlmsTxtEntity { AppId = appId, SpecificHost = string.Empty, LlmsContent = LlmsContent };
        _mockRepository.Setup(x => x.GetAllForSite(It.IsAny<string>())).Returns([LlmsTxtEntity]);

        // Act
        var result = _service.GetLlmsContent(appId, null);

        // Assert
        Assert.That(result, Is.EqualTo(LlmsContent));
    }

    [Test]
    public void GetLlmsContent_siteId_ReturnsNullForASiteWhenLlmsContentExistsButNoMatchingSpecificHostIsProvided()
    {
        // Arrange
        var appId = "appId";
        var LlmsContent = GetSavedLlms();
        var LlmsTxtEntity = new LlmsTxtEntity { AppId = appId, SpecificHost = "www.example.com", LlmsContent = LlmsContent };
        _mockRepository.Setup(x => x.GetAllForSite(It.IsAny<string>())).Returns([LlmsTxtEntity]);

        // Act
        var result = _service.GetLlmsContent(appId, "www.non-matching.com");

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetLlmsContent_siteId_ReturnsMatchingHostWhenBothDefaultAndDefinedHostExist()
    {
        // Arrange
        var appId = "appId";
        var LlmsContent = GetSavedLlms();
        var hostDefinedEntity = new LlmsTxtEntity { AppId = appId, SpecificHost = "www.example.com", LlmsContent = "Defined Robots" };
        var defaultEntity = new LlmsTxtEntity { AppId = appId, LlmsContent = "Default Robots" };
        _mockRepository.Setup(x => x.GetAllForSite(It.IsAny<string>())).Returns([hostDefinedEntity, defaultEntity]);

        // Act
        var result = _service.GetLlmsContent(appId, "www.example.com");

        // Assert
        Assert.That(result, Is.EqualTo(hostDefinedEntity.LlmsContent));
    }

    [Test]
    public void GetLlmsContent_siteId_ReturnsDefaultHostWhenBothDefaultAndDefinedHostExistForANonMatchingHost()
    {
        // Arrange
        var appId = "appId";
        var LlmsContent = GetSavedLlms();
        var hostDefinedEntity = new LlmsTxtEntity { AppId = appId, SpecificHost = "www.example.com", LlmsContent = "Defined Robots" };
        var defaultEntity = new LlmsTxtEntity { AppId = appId, LlmsContent = "Default Robots" };
        _mockRepository.Setup(x => x.GetAllForSite(It.IsAny<string>())).Returns([hostDefinedEntity, defaultEntity]);

        // Act
        var result = _service.GetLlmsContent(appId, "www.non-matching.com");

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
            AppId = "appId",
            LlmsContent = GetSavedLlms()
        };

        _mockApplicationDefinitionRepository.Setup(x => x.GetApplicationByIdAsync(It.IsAny<string>())).ReturnsAsync((ApplicationViewModel)null);

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
            AppId = "appId",
            LlmsContent = GetSavedLlms()
        };

        _mockApplicationDefinitionRepository.Setup(x => x.GetApplicationByIdAsync(It.IsAny<string>())).ReturnsAsync((ApplicationViewModel)null);

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
            AppId = "appId",
            LlmsContent = GetSavedLlms()
        };

        _mockApplicationDefinitionRepository.Setup(x => x.GetApplicationByIdAsync(It.IsAny<string>())).ReturnsAsync(new ApplicationViewModel { AppId = "appId" });

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
    [TestCase("f70719d4-adc6-4a06-8662-7c7e78ab3dbc", "appId1", "", false)]
    [TestCase("a841af98-cdbd-4e64-82b2-f31f3b0fe647", "appId1", "", true)]
    [TestCase("db107c5e-73ff-442f-93f3-bd99f56603f5", "appId1", "", true)]
    [TestCase("00000000-0000-0000-0000-000000000000", "appId1", "", true)]
    [TestCase("a841af98-cdbd-4e64-82b2-f31f3b0fe647", "appId1", "www.example.com", false)]
    [TestCase("db107c5e-73ff-442f-93f3-bd99f56603f5", "appId1", "www.example.com", true)]
    [TestCase("00000000-0000-0000-0000-000000000000", "appId1", "www.example.com", true)]
    [TestCase("db107c5e-73ff-442f-93f3-bd99f56603f5", "appId1", "www.non-matching.com", false)]
    public void DoesConflictExists_GivenTheRepositoryContainsAConflictingConfiguration_ThenTrueIsReturned(string id, string appId, string host, bool expectedValue)
    {
        // Arrange
        var savedRecords = new List<LlmsTxtEntity>
        {
            new()
            {
                Id = Guid.Parse("f70719d4-adc6-4a06-8662-7c7e78ab3dbc"),
                AppId = "appId1",
                SpecificHost = string.Empty,
                LlmsContent = GetSavedLlms()
            },
            new()
            {
                Id = Guid.Parse("a841af98-cdbd-4e64-82b2-f31f3b0fe647"),
                AppId = "appId1",
                SpecificHost = "www.example.com",
                LlmsContent = GetSavedLlms()
            }
        };

        var model = new SaveLlmsModel { Id = Guid.Parse(id), AppId = appId, SpecificHost = host, LlmsContent = GetSavedLlms() };
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
        _mockApplicationDefinitionRepository.Setup(x => x.GetApplicationByIdAsync(It.IsAny<string>())).ReturnsAsync((ApplicationViewModel)null);

        // Assert
        Assert.Throws<ArgumentException>(() => _service.GetDefault(string.Empty));
    }

    [Test]
    public void GetDefault_WhenPassedAValidSiteId_ButTheRepositoryDoesNotContainTheSite_ThenThrowsArgumentException()
    {
        // Arrange
        var appId = "appId";

        _mockApplicationDefinitionRepository.Setup(x => x.GetApplicationByIdAsync(It.IsAny<string>())).ReturnsAsync((ApplicationViewModel)null);

        // Assert
        Assert.Throws<ArgumentException>(() => _service.GetDefault(appId));
    }

    [Test]
    public void GetDefault_WhenPassedAValidSiteId_ThenReturnsAValidSiteRobotsViewModel()
    {
        // Arrange
        var appId = "appId";
        var application = new ApplicationViewModel { AppId = appId };

        _mockApplicationDefinitionRepository.Setup(x => x.GetApplicationByIdAsync(It.IsAny<string>())).ReturnsAsync(application);

        // Act
        var result = _service.GetDefault(appId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(Guid.Empty));
        Assert.That(result.AppId, Is.EqualTo(appId));
        Assert.That(result.SpecificHost, Is.Null);
    }

    [Test]
    public void GetAll_WhenTheLlmsContentRepositoryHasNoRecords_ThenAnEmptyCollectionShouldBeReturned()
    {
        // Arrange
        var sites = new List<ApplicationViewModel>
        {
            new() { AppId = "appId1" },
            new() { AppId = "appId2" }
        };

        _mockRepository.Setup(x => x.GetAll()).Returns([]);
        _mockApplicationDefinitionRepository.Setup(x => x.GetAllApplicationsAsync()).ReturnsAsync(sites);

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
        var defaultHosts = new List<HostViewModel> { new() { DisplayName = "Default", HostName = string.Empty } };
        var sites = new List<ApplicationViewModel>
        {
            new() { AppId = "appId1", AvailableHosts = defaultHosts },
            new() { AppId = "appId2", AvailableHosts = defaultHosts }
        };

        var robotsEntities = new List<LlmsTxtEntity>
        {
            new() { AppId = sites[0].AppId },
            new() { AppId = sites[1].AppId }
        };

        _mockRepository.Setup(x => x.GetAll()).Returns(robotsEntities);
        _mockApplicationDefinitionRepository.Setup(x => x.GetAllApplicationsAsync()).ReturnsAsync(sites);

        // Act
        var result = _service.GetAll();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result[0].AppId, Is.EqualTo(sites[0].AppId));
        Assert.That(result[0].AvailableHosts[0].DisplayName, Is.EqualTo("Default"));
        Assert.That(result[0].AvailableHosts[0].HostName, Is.EqualTo(string.Empty));
        Assert.That(result[1].AppId, Is.EqualTo(sites[1].AppId));
        Assert.That(result[1].AvailableHosts[0].DisplayName, Is.EqualTo("Default"));
        Assert.That(result[1].AvailableHosts[0].HostName, Is.EqualTo(string.Empty));
    }

    [Test]
    public void GetAll_WhenThereAreLlmsContentForSpecificHosts_ThenAnEntryIsReturnedForEachItem()
    {
        // Arrange
        var sites = new List<ApplicationViewModel>
        {
            new() { AppId = "appId1", AppName = "Site 1", AvailableHosts = [new() { DisplayName = "www.exampleone.com", HostName = "www.exampleone.com" }] },
            new() { AppId = "appId2", AppName = "Site 2", AvailableHosts = [new() { DisplayName = "www.exampletwo.com", HostName = "www.exampletwo.com" }] },
        };

        var robotsEntities = new List<LlmsTxtEntity>
        {
            new() { AppId = sites[0].AppId, SpecificHost = "www.exampleone.com" },
            new() { AppId = sites[1].AppId, SpecificHost = "www.exampletwo.com" }
        };

        _mockRepository.Setup(x => x.GetAll()).Returns(robotsEntities);
        _mockApplicationDefinitionRepository.Setup(x => x.GetAllApplicationsAsync()).ReturnsAsync(sites);

        // Act
        var result = _service.GetAll();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Has.Count.EqualTo(2));

        Assert.That(result[0].AppId, Is.EqualTo(sites[0].AppId));
        Assert.That(result[0].AppName, Is.EqualTo(sites[0].AppName));
        Assert.That(result[0].SpecificHost, Is.EqualTo("www.exampleone.com"));
        Assert.That(result[0].AvailableHosts[0].DisplayName, Is.EqualTo("www.exampleone.com"));
        Assert.That(result[0].AvailableHosts[0].HostName, Is.EqualTo("www.exampleone.com"));

        Assert.That(result[1].AppId, Is.EqualTo(sites[1].AppId));
        Assert.That(result[1].AppName, Is.EqualTo(sites[1].AppName));
        Assert.That(result[1].SpecificHost, Is.EqualTo("www.exampletwo.com"));
        Assert.That(result[1].AvailableHosts[0].DisplayName, Is.EqualTo("www.exampletwo.com"));
        Assert.That(result[1].AvailableHosts[0].HostName, Is.EqualTo("www.exampletwo.com"));
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
        var LlmsTxtEntity = new LlmsTxtEntity { AppId = "nonMatchingAppId" };
        _mockRepository.Setup(x => x.Get(It.IsAny<Guid>())).Returns(LlmsTxtEntity);
        _mockApplicationDefinitionRepository.Setup(x => x.GetAllApplicationsAsync()).ReturnsAsync([]);

        // Assert
        Assert.Throws<RobotsEntityNotFoundException>(() => _service.Get(id));
    }

    [Test]
    public void Get_WhenPassedAnIdForALlmsTxtEntityThatExists_ThenReturnsTheModel()
    {
        // Arrange
        var id = Guid.NewGuid();
        var appId = "appId";
        var llmsTxtEntity = new LlmsTxtEntity { AppId = appId };
        _mockRepository.Setup(x => x.Get(It.IsAny<Guid>())).Returns(llmsTxtEntity);
        _mockApplicationDefinitionRepository.Setup(x => x.GetAllApplicationsAsync()).ReturnsAsync(
        [
            new() { AppId = appId }
        ]);

        // Act
        var result = _service.Get(id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(llmsTxtEntity.Id.ExternalId));
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
