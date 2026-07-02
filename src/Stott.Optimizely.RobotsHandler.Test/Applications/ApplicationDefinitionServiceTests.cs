using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using EPiServer.Applications;
using EPiServer.Core;

using Moq;

using NUnit.Framework;

using Stott.Optimizely.RobotsHandler.Applications;
using Stott.Optimizely.RobotsHandler.Test.TestCases;

namespace Stott.Optimizely.RobotsHandler.Test.Applications;

[TestFixture]
public sealed class ApplicationDefinitionServiceTests
{
    private Mock<IApplicationRepository> _mockRepository;

    private ApplicationDefinitionService _service;

    [SetUp]
    public void SetUp()
    {
        _mockRepository = new Mock<IApplicationRepository>();

        _service = new ApplicationDefinitionService(_mockRepository.Object);
    }

    [Test]
    public async Task GetAllApplicationsAsync_WhenTheRepositoryReturnsNoApplications_ThenAnEmptyCollectionIsReturned()
    {
        // Arrange
        _mockRepository.Setup(x => x.ListAsync(It.IsAny<CancellationToken>())).ReturnsAsync([]);

        // Act
        var result = await _service.GetAllApplicationsAsync();

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetAllApplicationsAsync_MapsAWebsiteToAViewModelUsingItsNameDisplayNameAndHosts()
    {
        // Arrange
        var website = new Website("app-one", ContentReference.EmptyReference) { DisplayName = "App One" };
        website.Hosts.Add(new ApplicationHost("www.example.com"));
        _mockRepository.Setup(x => x.ListAsync(It.IsAny<CancellationToken>())).ReturnsAsync([website]);

        // Act
        var result = (await _service.GetAllApplicationsAsync()).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].AppId, Is.EqualTo("app-one"));
        Assert.That(result[0].AppName, Is.EqualTo("App One"));
        Assert.That(result[0].AvailableHosts, Has.Count.EqualTo(2));
        Assert.That(result[0].AvailableHosts[0].DisplayName, Is.EqualTo("Default"));
        Assert.That(result[0].AvailableHosts[1].HostName, Is.EqualTo("www.example.com"));
    }

    [Test]
    public async Task GetAllApplicationsAsync_MapsAnInProcessWebsiteToAViewModel()
    {
        // Arrange
        var website = new InProcessWebsite("app-two", ContentReference.EmptyReference) { DisplayName = "App Two" };
        _mockRepository.Setup(x => x.ListAsync(It.IsAny<CancellationToken>())).ReturnsAsync([website]);

        // Act
        var result = (await _service.GetAllApplicationsAsync()).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].AppId, Is.EqualTo("app-two"));
        Assert.That(result[0].AppName, Is.EqualTo("App Two"));
    }

    [Test]
    [TestCaseSource(typeof(CommonTestCases), nameof(CommonTestCases.EmptyStrings))]
    public async Task GetApplicationByIdAsync_WhenGivenAnEmptyAppId_ThenReturnsNullWithoutQueryingTheRepository(string appId)
    {
        // Act
        var result = await _service.GetApplicationByIdAsync(appId);

        // Assert
        Assert.That(result, Is.Null);
        _mockRepository.Verify(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task GetApplicationByIdAsync_WhenTheRepositoryReturnsAWebsite_ThenItIsMappedToAViewModel()
    {
        // Arrange
        var website = new Website("app-one", ContentReference.EmptyReference) { DisplayName = "App One" };
        website.Hosts.Add(new ApplicationHost("www.example.com"));
        _mockRepository.Setup(x => x.GetAsync("app-one", It.IsAny<CancellationToken>())).ReturnsAsync(website);

        // Act
        var result = await _service.GetApplicationByIdAsync("app-one");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AppId, Is.EqualTo("app-one"));
        Assert.That(result.AppName, Is.EqualTo("App One"));
        Assert.That(result.AvailableHosts, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task GetApplicationByIdAsync_WhenTheRepositoryReturnsAnInProcessWebsite_ThenItIsMappedToAViewModel()
    {
        // Arrange
        var website = new InProcessWebsite("app-two", ContentReference.EmptyReference) { DisplayName = "App Two" };
        _mockRepository.Setup(x => x.GetAsync("app-two", It.IsAny<CancellationToken>())).ReturnsAsync(website);

        // Act
        var result = await _service.GetApplicationByIdAsync("app-two");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AppId, Is.EqualTo("app-two"));
    }

    [Test]
    public async Task GetApplicationByIdAsync_WhenTheRepositoryReturnsNull_ThenReturnsNull()
    {
        // Arrange
        _mockRepository.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((Application)null);

        // Act
        var result = await _service.GetApplicationByIdAsync("does-not-exist");

        // Assert
        Assert.That(result, Is.Null);
    }
}