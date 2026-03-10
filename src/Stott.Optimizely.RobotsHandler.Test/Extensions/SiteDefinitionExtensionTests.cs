using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Applications;
using EPiServer.Web;

using NUnit.Framework;
using Stott.Optimizely.RobotsHandler.Applications;
using Stott.Optimizely.RobotsHandler.Extensions;

namespace Stott.Optimizely.RobotsHandler.Test.Extensions;

[TestFixture]
public sealed class ApplicationMapperTests
{
    [Test]
    public void ToHostSummaries_ReturnsDefaultHost_WhenHostDefinitionsIsNull()
    {
        // Arrange
        IList<ApplicationHost> hostDefinitions = null;

        // Act
        var result = ApplicationMapper.CreateHostSummaries(hostDefinitions);

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result.First().DisplayName, Is.EqualTo("Default"));
        Assert.That(result.First().HostName, Is.EqualTo(string.Empty));
    }

    [Test]
    public void ToHostSummaries_ReturnsDefaultHost_WhenHostDefinitionsIsEmpty()
    {
        // Arrange
        IList<ApplicationHost> hostDefinitions = new List<ApplicationHost>();

        // Act
        var result = ApplicationMapper.CreateHostSummaries(hostDefinitions);

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result.First().DisplayName, Is.EqualTo("Default"));
        Assert.That(result.First().HostName, Is.EqualTo(string.Empty));
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
        Assert.That(result[1].DisplayName, Is.EqualTo("host1.com"));
        Assert.That(result[1].HostName, Is.EqualTo("host1.com"));
        Assert.That(result[2].DisplayName, Is.EqualTo("host2.com"));
        Assert.That(result[2].HostName, Is.EqualTo("host2.com"));
    }

    [Test]
    public void ToHostSummaries_ExcludesHostsWithoutUrl()
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
        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result[0].DisplayName, Is.EqualTo("Default"));
        Assert.That(result[0].HostName, Is.EqualTo(string.Empty));
        Assert.That(result[1].DisplayName, Is.EqualTo("host1.com"));
        Assert.That(result[1].HostName, Is.EqualTo("host1.com"));
    }
}
