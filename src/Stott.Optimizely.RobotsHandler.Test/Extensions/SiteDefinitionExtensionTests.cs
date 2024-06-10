using System.Collections.Generic;
using System.Linq;

using EPiServer.Web;

using NUnit.Framework;

using Stott.Optimizely.RobotsHandler.Extensions;

namespace Stott.Optimizely.RobotsHandler.Test.Extensions;

[TestFixture]
public sealed class SiteDefinitionExtensionTests
{
    [Test]
    public void ToHostSummaries_ReturnsDefaultHost_WhenHostDefinitionsIsNull()
    {
        // Arrange
        IList<HostDefinition> hostDefinitions = null;

        // Act
        var result = hostDefinitions.ToHostSummaries().ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result.First().DisplayName, Is.EqualTo("Default"));
        Assert.That(result.First().HostName, Is.EqualTo(string.Empty));
    }

    [Test]
    public void ToHostSummaries_ReturnsDefaultHost_WhenHostDefinitionsIsEmpty()
    {
        // Arrange
        IList<HostDefinition> hostDefinitions = new List<HostDefinition>();

        // Act
        var result = hostDefinitions.ToHostSummaries().ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result.First().DisplayName, Is.EqualTo("Default"));
        Assert.That(result.First().HostName, Is.EqualTo(string.Empty));
    }

    [Test]
    public void ToHostSummaries_ReturnsDefaultAndHosts_WhenHostDefinitionsIsNotEmpty()
    {
        // Arrange
        IList<HostDefinition> hostDefinitions = new List<HostDefinition>
        {
            new() { Name = "host1.com" },
            new() { Name = "host2.com" }
        };

        // Act
        var result = hostDefinitions.ToHostSummaries().ToList();

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
        IList<HostDefinition> hostDefinitions = new List<HostDefinition>
        {
            new() { Name = "host1.com" },
            new() { Name = "*" }
        };

        // Act
        var result = hostDefinitions.ToHostSummaries().ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result[0].DisplayName, Is.EqualTo("Default"));
        Assert.That(result[0].HostName, Is.EqualTo(string.Empty));
        Assert.That(result[1].DisplayName, Is.EqualTo("host1.com"));
        Assert.That(result[1].HostName, Is.EqualTo("host1.com"));
    }
}
