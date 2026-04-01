using System;
using System.Collections.Generic;
using System.Text;

using Moq;

using NUnit.Framework;

using Stott.Optimizely.RobotsHandler.Applications;
using Stott.Optimizely.RobotsHandler.Models;
using Stott.Optimizely.RobotsHandler.Robots;

namespace Stott.Optimizely.RobotsHandler.Test.Robots;

[TestFixture]
public sealed class RobotsContentServiceTests
{
    private Mock<IApplicationDefinitionService> _mockAppService;

    private Mock<IRobotsContentRepository> _mockRobotsContentRepository;

    private RobotsContentService _robotsContentService;

    [SetUp]
    public void SetUp()
    {
        _mockAppService = new Mock<IApplicationDefinitionService>();
        _mockAppService.Setup(x => x.GetAllApplicationsAsync()).ReturnsAsync([]);

        _mockRobotsContentRepository = new Mock<IRobotsContentRepository>();

        _robotsContentService = new RobotsContentService(_mockAppService.Object, _mockRobotsContentRepository.Object);
    }

    [Test]
    public void GetRobotsContent_siteId_ReturnsDefaultRobotsForAValidSiteWhenRobotsContentDoesNotExist()
    {
        // Arrange
        var appId = Guid.NewGuid().ToString();
        _mockRobotsContentRepository.Setup(x => x.Get(It.IsAny<Guid>())).Returns((RobotsEntity)null);
        _mockRobotsContentRepository.Setup(x => x.GetAllForSite(It.IsAny<string>())).Returns([]);

        // Act
        var robotsContent = _robotsContentService.GetRobotsContent(appId, null);

        // Assert
        Assert.That(robotsContent, Is.EqualTo(_robotsContentService.GetDefaultRobotsContent()));
    }

    [Test]
    public void GetRobotsContent_siteId_ReturnsRobotsContentForASpecificHostWhenRobotsContentExists()
    {
        // Arrange
        var appId = Guid.NewGuid().ToString();
        var robotsContent = GetSavedRobots();
        var robotsEntity = new RobotsEntity { AppId = appId, SpecificHost = "www.example.com", RobotsContent = robotsContent };
        _mockRobotsContentRepository.Setup(x => x.GetAllForSite(It.IsAny<string>())).Returns([robotsEntity]);

        // Act
        var result = _robotsContentService.GetRobotsContent(appId, "www.example.com");

        // Assert
        Assert.That(result, Is.EqualTo(robotsContent));
    }

    [Test]
    public void GetRobotsContent_siteId_ReturnsDefaultRobotsForASiteWhenRobotsContentExistsButNoSpecificHostIsProvided()
    {
        // Arrange
        var appId = Guid.NewGuid().ToString();
        var robotsContent = GetSavedRobots();
        var robotsEntity = new RobotsEntity { AppId = appId, SpecificHost = string.Empty, RobotsContent = robotsContent };
        _mockRobotsContentRepository.Setup(x => x.GetAllForSite(It.IsAny<string>())).Returns([robotsEntity]);

        // Act
        var result = _robotsContentService.GetRobotsContent(appId, null);

        // Assert
        Assert.That(result, Is.EqualTo(robotsContent));
    }

    [Test]
    public void GetRobotsContent_siteId_ReturnsDefaultRobotsForASiteWhenRobotsContentExistsButNoMatchingSpecificHostIsProvided()
    {
        // Arrange
        var appId = Guid.NewGuid().ToString();
        var robotsContent = GetSavedRobots();
        var robotsEntity = new RobotsEntity { AppId = appId, SpecificHost = "www.example.com", RobotsContent = robotsContent };
        _mockRobotsContentRepository.Setup(x => x.GetAllForSite(It.IsAny<string>())).Returns([robotsEntity]);

        // Act
        var result = _robotsContentService.GetRobotsContent(appId, "www.non-matching.com");

        // Assert
        Assert.That(result, Is.EqualTo(_robotsContentService.GetDefaultRobotsContent()));
    }

    [Test]
    public void GetRobotsContent_siteId_ReturnsMatchingHostWhenBothDefaultAndDefinedHostExist()
    {
        // Arrange
        var appId = Guid.NewGuid().ToString();
        var robotsContent = GetSavedRobots();
        var hostDefinedEntity = new RobotsEntity { AppId = appId, SpecificHost = "www.example.com", RobotsContent = "Defined Robots" };
        var defaultEntity = new RobotsEntity { AppId = appId, RobotsContent = "Default Robots" };
        _mockRobotsContentRepository.Setup(x => x.GetAllForSite(It.IsAny<string>())).Returns([hostDefinedEntity, defaultEntity]);

        // Act
        var result = _robotsContentService.GetRobotsContent(appId, "www.example.com");

        // Assert
        Assert.That(result, Is.EqualTo(hostDefinedEntity.RobotsContent));
    }

    [Test]
    public void GetRobotsContent_siteId_ReturnsDefaultHostWhenBothDefaultAndDefinedHostExistForANonMatchingHost()
    {
        // Arrange
        var appId = Guid.NewGuid().ToString();
        var robotsContent = GetSavedRobots();
        var hostDefinedEntity = new RobotsEntity { AppId = appId, SpecificHost = "www.example.com", RobotsContent = "Defined Robots" };
        var defaultEntity = new RobotsEntity { AppId = appId, RobotsContent = "Default Robots" };
        _mockRobotsContentRepository.Setup(x => x.GetAllForSite(It.IsAny<string>())).Returns([hostDefinedEntity, defaultEntity]);

        // Act
        var result = _robotsContentService.GetRobotsContent(appId, "www.non-matching.com");

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
            AppId = null,
            RobotsContent = GetSavedRobots()
        };

        _mockAppService.Setup(x => x.GetApplicationByIdAsync(It.IsAny<string>())).ReturnsAsync((ApplicationViewModel)null);

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
            AppId = "some-app-id",
            RobotsContent = GetSavedRobots()
        };

        _mockAppService.Setup(x => x.GetApplicationByIdAsync(It.IsAny<string>())).ReturnsAsync((ApplicationViewModel)null);

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
            AppId = "some-app-id",
            RobotsContent = GetSavedRobots()
        };

        _mockAppService.Setup(x => x.GetApplicationByIdAsync(It.IsAny<string>())).ReturnsAsync(new ApplicationViewModel { AppId = "some-app-id" });

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
    [TestCase("f70719d4-adc6-4a06-8662-7c7e78ab3dbc", "5645bc86", "", false)]
    [TestCase("a841af98-cdbd-4e64-82b2-f31f3b0fe647", "5645bc86", "", true)]
    [TestCase("db107c5e-73ff-442f-93f3-bd99f56603f5", "5645bc86", "", true)]
    [TestCase("00000000-0000-0000-0000-000000000000", "5645bc86", "", true)]
    [TestCase("a841af98-cdbd-4e64-82b2-f31f3b0fe647", "5645bc86", "www.example.com", false)]
    [TestCase("db107c5e-73ff-442f-93f3-bd99f56603f5", "5645bc86", "www.example.com", true)]
    [TestCase("00000000-0000-0000-0000-000000000000", "5645bc86", "www.example.com", true)]
    [TestCase("db107c5e-73ff-442f-93f3-bd99f56603f5", "5645bc86", "www.non-matching.com", false)]
    public void DoesConflictExists_GivenTheRepositoryContainsAConflictingConfiguration_ThenTrueIsReturned(string id, string appId, string host, bool expectedValue)
    {
        // Arrange
        var savedRecords = new List<RobotsEntity>
        {
            new()
            {
                Id = Guid.Parse("f70719d4-adc6-4a06-8662-7c7e78ab3dbc"),
                AppId = "5645bc86",
                SpecificHost = string.Empty,
                RobotsContent = GetSavedRobots()
            },
            new()
            {
                Id = Guid.Parse("a841af98-cdbd-4e64-82b2-f31f3b0fe647"),
                AppId = "5645bc86",
                SpecificHost = "www.example.com",
                RobotsContent = GetSavedRobots()
            }
        };

        var model = new SaveRobotsModel { Id = Guid.Parse(id), AppId = appId, SpecificHost = host, RobotsContent = GetSavedRobots() };
        _mockRobotsContentRepository.Setup(x => x.GetAll()).Returns(savedRecords);

        // Act
        var result = _robotsContentService.DoesConflictExists(model);

        // Assert
        Assert.That(result, Is.EqualTo(expectedValue));
    }

    [Test]
    public void GetDefault_WhenPassedAnInvalidAppId_ThenThrowsArgumentException()
    {
        // Arrange
        _mockAppService.Setup(x => x.GetApplicationByIdAsync(It.IsAny<string>())).ReturnsAsync((ApplicationViewModel)null);

        // Assert
        Assert.Throws<ArgumentException>(() => _robotsContentService.GetDefault(null));
    }

    [Test]
    public void GetDefault_WhenPassedAValidAppId_ButTheServiceDoesNotContainTheApplication_ThenThrowsArgumentException()
    {
        // Arrange
        var appId = Guid.NewGuid().ToString();

        _mockAppService.Setup(x => x.GetApplicationByIdAsync(It.IsAny<string>())).ReturnsAsync((ApplicationViewModel)null);

        // Assert
        Assert.Throws<ArgumentException>(() => _robotsContentService.GetDefault(appId));
    }

    [Test]
    public void GetDefault_WhenPassedAValidAppId_ThenReturnsAValidSiteRobotsViewModel()
    {
        // Arrange
        var appId = Guid.NewGuid().ToString();
        var application = new ApplicationViewModel { AppId = appId };

        _mockAppService.Setup(x => x.GetApplicationByIdAsync(It.IsAny<string>())).ReturnsAsync(application);

        // Act
        var result = _robotsContentService.GetDefault(appId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(Guid.Empty));
        Assert.That(result.AppId, Is.EqualTo(appId));
        Assert.That(result.SpecificHost, Is.Null);
    }

    [Test]
    public void GetAll_WhenTheRobotsContentRepositoryHasNoRecords_ThenDefaultRecordsShouldBeReturnedForEachSite()
    {
        // Arrange
        var defaultHosts = new List<HostViewModel> { new() { DisplayName = "Default", HostName = string.Empty } };
        var applications = new List<ApplicationViewModel>
        {
            new() { AppId = Guid.NewGuid().ToString(), AvailableHosts = defaultHosts },
            new() { AppId = Guid.NewGuid().ToString(), AvailableHosts = defaultHosts }
        };

        _mockRobotsContentRepository.Setup(x => x.GetAll()).Returns([]);
        _mockAppService.Setup(x => x.GetAllApplicationsAsync()).ReturnsAsync(applications);

        // Act
        var result = _robotsContentService.GetAll();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result[0].AppId, Is.EqualTo(applications[0].AppId));
        Assert.That(result[0].AvailableHosts[0].DisplayName, Is.EqualTo("Default"));
        Assert.That(result[0].AvailableHosts[0].HostName, Is.EqualTo(string.Empty));
        Assert.That(result[1].AppId, Is.EqualTo(applications[1].AppId));
        Assert.That(result[1].AvailableHosts[0].DisplayName, Is.EqualTo("Default"));
        Assert.That(result[1].AvailableHosts[0].HostName, Is.EqualTo(string.Empty));
    }

    [Test]
    public void GetAll_WhenTheRobotsContentRepositoryHasRecords_ThenTheRecordsShouldBeReturnedForEachSite()
    {
        // Arrange
        var defaultHosts = new List<HostViewModel> { new() { DisplayName = "Default", HostName = string.Empty } };
        var applications = new List<ApplicationViewModel>
        {
            new() { AppId = Guid.NewGuid().ToString(), AvailableHosts = defaultHosts },
            new() { AppId = Guid.NewGuid().ToString(), AvailableHosts = defaultHosts }
        };

        var robotsEntities = new List<RobotsEntity>
        {
            new() { AppId = applications[0].AppId },
            new() { AppId = applications[1].AppId }
        };

        _mockRobotsContentRepository.Setup(x => x.GetAll()).Returns(robotsEntities);
        _mockAppService.Setup(x => x.GetAllApplicationsAsync()).ReturnsAsync(applications);

        // Act
        var result = _robotsContentService.GetAll();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result[0].AppId, Is.EqualTo(applications[0].AppId));
        Assert.That(result[0].AvailableHosts[0].DisplayName, Is.EqualTo("Default"));
        Assert.That(result[0].AvailableHosts[0].HostName, Is.EqualTo(string.Empty));
        Assert.That(result[1].AppId, Is.EqualTo(applications[1].AppId));
        Assert.That(result[1].AvailableHosts[0].DisplayName, Is.EqualTo("Default"));
        Assert.That(result[1].AvailableHosts[0].HostName, Is.EqualTo(string.Empty));
    }

    [Test]
    public void GetAll_WhenThereAreRobotsContentForSpecificHosts_ThenTheSpecificHostsShouldBeReturned()
    {
        // Arrange
        var applications = new List<ApplicationViewModel>
        {
            new() { AppId = Guid.NewGuid().ToString(), AppName = "Site 1", AvailableHosts = [new() { DisplayName = "www.exampleone.com", HostName = "www.exampleone.com" }] },
            new() { AppId = Guid.NewGuid().ToString(), AppName = "Site 2", AvailableHosts = [new() { DisplayName = "www.exampletwo.com", HostName = "www.exampletwo.com" }] },
        };

        var robotsEntities = new List<RobotsEntity>
        {
            new() { AppId = applications[0].AppId, SpecificHost = "www.exampleone.com" },
            new() { AppId = applications[1].AppId, SpecificHost = "www.exampletwo.com" }
        };

        _mockRobotsContentRepository.Setup(x => x.GetAll()).Returns(robotsEntities);
        _mockAppService.Setup(x => x.GetAllApplicationsAsync()).ReturnsAsync(applications);

        // Act
        var result = _robotsContentService.GetAll();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Has.Count.EqualTo(4));

        Assert.That(result[0].AppId, Is.EqualTo(applications[0].AppId));
        Assert.That(result[0].AppName, Is.EqualTo(applications[0].AppName));
        Assert.That(result[0].SpecificHost, Is.Null);
        Assert.That(result[0].CanDelete, Is.False);
        Assert.That(result[0].AvailableHosts[0].DisplayName, Is.EqualTo("www.exampleone.com"));
        Assert.That(result[0].AvailableHosts[0].HostName, Is.EqualTo("www.exampleone.com"));

        Assert.That(result[1].AppId, Is.EqualTo(applications[0].AppId));
        Assert.That(result[1].AppName, Is.EqualTo(applications[0].AppName));
        Assert.That(result[1].SpecificHost, Is.EqualTo("www.exampleone.com"));
        Assert.That(result[1].CanDelete, Is.True);
        Assert.That(result[1].AvailableHosts[0].DisplayName, Is.EqualTo("www.exampleone.com"));
        Assert.That(result[1].AvailableHosts[0].HostName, Is.EqualTo("www.exampleone.com"));

        Assert.That(result[2].AppId, Is.EqualTo(applications[1].AppId));
        Assert.That(result[2].AppName, Is.EqualTo(applications[1].AppName));
        Assert.That(result[2].SpecificHost, Is.Null);
        Assert.That(result[2].CanDelete, Is.False);
        Assert.That(result[2].AvailableHosts[0].DisplayName, Is.EqualTo("www.exampletwo.com"));
        Assert.That(result[2].AvailableHosts[0].HostName, Is.EqualTo("www.exampletwo.com"));

        Assert.That(result[3].AppId, Is.EqualTo(applications[1].AppId));
        Assert.That(result[3].AppName, Is.EqualTo(applications[1].AppName));
        Assert.That(result[3].SpecificHost, Is.EqualTo("www.exampletwo.com"));
        Assert.That(result[3].CanDelete, Is.True);
        Assert.That(result[3].AvailableHosts[0].DisplayName, Is.EqualTo("www.exampletwo.com"));
        Assert.That(result[3].AvailableHosts[0].HostName, Is.EqualTo("www.exampletwo.com"));
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
        var robotsEntity = new RobotsEntity { AppId = Guid.NewGuid().ToString() };
        _mockRobotsContentRepository.Setup(x => x.Get(It.IsAny<Guid>())).Returns(robotsEntity);
        _mockAppService.Setup(x => x.GetAllApplicationsAsync()).ReturnsAsync([]);

        // Assert
        Assert.Throws<RobotsEntityNotFoundException>(() => _robotsContentService.Get(id));
    }

    [Test]
    public void Get_WhenPassedAnIdForARobotsEntityThatExists_ThenReturnsTheModel()
    {
        // Arrange
        var id = Guid.NewGuid();
        var appId = Guid.NewGuid().ToString();
        var robotsEntity = new RobotsEntity { AppId = appId };
        _mockRobotsContentRepository.Setup(x => x.Get(It.IsAny<Guid>())).Returns(robotsEntity);
        _mockAppService.Setup(x => x.GetAllApplicationsAsync()).ReturnsAsync([new() { AppId = appId }]);

        // Act
        var result = _robotsContentService.Get(id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(robotsEntity.Id.ExternalId));
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
