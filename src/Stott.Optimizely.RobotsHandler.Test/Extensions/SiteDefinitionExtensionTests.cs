using System.Collections.Generic;
using System.Linq;
using EPiServer.Applications;

using NUnit.Framework;
using Stott.Optimizely.RobotsHandler.Applications;

namespace Stott.Optimizely.RobotsHandler.Test.Extensions;

[TestFixture]
public sealed class ApplicationMapperTests
{
    [Test]
    public void ToHostSummaries_ReturnsEmpty_WhenHostDefinitionsIsNull()
    {
        // Arrange
        IList<ApplicationHost> hostDefinitions = null;

        // Act
        var result = ApplicationMapper.CreateHostSummaries(hostDefinitions).ToList();

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void ToHostSummaries_ReturnsEmpty_WhenHostDefinitionsIsEmpty()
    {
        // Arrange
        IList<ApplicationHost> hostDefinitions = new List<ApplicationHost>();

        // Act
        var result = ApplicationMapper.CreateHostSummaries(hostDefinitions).ToList();

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void ToHostSummaries_ReturnsDefaultAndHosts_WhenHostDefinitionsIsNotEmpty()
    {
        // Arrange
        IList<ApplicationHost> hostDefinitions = new List<ApplicationHost>
        {
            new ApplicationHost("host1.com"),
            new ApplicationHost("host2.com")
        };

        // Act
        var result = ApplicationMapper.CreateHostSummaries(hostDefinitions).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(3));
        Assert.That(result[0].DisplayName, Is.EqualTo("Default"));
        Assert.That(result[0].HostName, Is.EqualTo(string.Empty));
        Assert.That(result[1].DisplayName, Is.EqualTo("http://host1.com/"));
        Assert.That(result[1].HostName, Is.EqualTo("http://host1.com/"));
        Assert.That(result[2].DisplayName, Is.EqualTo("http://host2.com/"));
        Assert.That(result[2].HostName, Is.EqualTo("http://host2.com/"));
    }

    [Test]
    public void ToHostSummaries_IncludesAllHostsWithUrls()
    {
        // Arrange
        IList<ApplicationHost> hostDefinitions = new List<ApplicationHost>
        {
            new ApplicationHost("host1.com"),
            new ApplicationHost("host2.com")
        };

        // Act
        var result = ApplicationMapper.CreateHostSummaries(hostDefinitions).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(3));
        Assert.That(result[0].DisplayName, Is.EqualTo("Default"));
        Assert.That(result[0].HostName, Is.EqualTo(string.Empty));
        Assert.That(result[1].DisplayName, Is.EqualTo("http://host1.com/"));
        Assert.That(result[1].HostName, Is.EqualTo("http://host1.com/"));
    }
}
