using System.Collections.Generic;
using System.Linq;

using EPiServer.Applications;

using NUnit.Framework;

using Stott.Optimizely.RobotsHandler.Applications;

namespace Stott.Optimizely.RobotsHandler.Test.Applications;

[TestFixture]
public sealed class ApplicationMapperTests
{
    [Test]
    public void CreateHostSummaries_string_ReturnsASingleSummaryWithTheProvidedNameAndAnEmptyHostName()
    {
        // Act
        var result = ApplicationMapper.CreateHostSummaries("www.example.com");

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].DisplayName, Is.EqualTo("www.example.com"));
        Assert.That(result[0].HostName, Is.EqualTo(string.Empty));
    }

    [Test]
    public void CreateHostSummaries_hosts_WhenGivenNull_ThenReturnsAnEmptyCollection()
    {
        // Act
        var result = ApplicationMapper.CreateHostSummaries((IList<ApplicationHost>)null).ToList();

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void CreateHostSummaries_hosts_WhenGivenAnEmptyList_ThenReturnsAnEmptyCollection()
    {
        // Act
        var result = ApplicationMapper.CreateHostSummaries(new List<ApplicationHost>()).ToList();

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void CreateHostSummaries_hosts_WhenGivenHosts_ThenTheFirstSummaryIsTheDefaultHostWithAnEmptyHostName()
    {
        // Arrange
        var hosts = new List<ApplicationHost> { new("www.example.com") };

        // Act
        var result = ApplicationMapper.CreateHostSummaries(hosts).ToList();

        // Assert
        Assert.That(result[0].DisplayName, Is.EqualTo("Default"));
        Assert.That(result[0].HostName, Is.EqualTo(string.Empty));
    }

    [Test]
    public void CreateHostSummaries_hosts_WhenGivenAHost_ThenTheHostSummaryUsesTheAbsoluteUrlAndSanitisedDomain()
    {
        // Arrange
        var hosts = new List<ApplicationHost> { new("www.example.com") };

        // Act
        var result = ApplicationMapper.CreateHostSummaries(hosts).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result[1].DisplayName, Is.EqualTo("http://www.example.com/"));
        Assert.That(result[1].HostName, Is.EqualTo("www.example.com"));
    }

    [Test]
    public void CreateHostSummaries_hosts_WhenTheHostHasANonDefaultPort_ThenTheSanitisedDomainIncludesThePort()
    {
        // Arrange
        var hosts = new List<ApplicationHost> { new("www.example.com:8080") };

        // Act
        var result = ApplicationMapper.CreateHostSummaries(hosts).ToList();

        // Assert
        Assert.That(result[1].DisplayName, Is.EqualTo("http://www.example.com:8080/"));
        Assert.That(result[1].HostName, Is.EqualTo("www.example.com:8080"));
    }

    [Test]
    public void CreateHostSummaries_hosts_WhenGivenMultipleHosts_ThenADefaultSummaryPlusOneSummaryPerHostIsReturned()
    {
        // Arrange
        var hosts = new List<ApplicationHost>
        {
            new("www.example.com"),
            new("www.example.org")
        };

        // Act
        var result = ApplicationMapper.CreateHostSummaries(hosts).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(3));
        Assert.That(result[0].DisplayName, Is.EqualTo("Default"));
        Assert.That(result[1].HostName, Is.EqualTo("www.example.com"));
        Assert.That(result[2].HostName, Is.EqualTo("www.example.org"));
    }
}
