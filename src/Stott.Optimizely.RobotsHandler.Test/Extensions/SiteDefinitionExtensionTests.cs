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
    public void ToHostSummaries_ReturnsDefaultHost_WhenHostDefinitionsIsEmpty()
    {
        // Arrange
        IList<HostDefinition> hostDefinitions = new List<HostDefinition>();

        // Act
        var result = hostDefinitions.ToHostSummaries().ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result.First().Key, Is.EqualTo("Default"));
        Assert.That(result.First().Value, Is.EqualTo(string.Empty));
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
        Assert.That(result[0].Key, Is.EqualTo("Default"));
        Assert.That(result[0].Value, Is.EqualTo(string.Empty));
        Assert.That(result[1].Key, Is.EqualTo("host1.com"));
        Assert.That(result[1].Value, Is.EqualTo("http://host1.com/"));
        Assert.That(result[2].Key, Is.EqualTo("host2.com"));
        Assert.That(result[2].Value, Is.EqualTo("http://host2.com/"));
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
        Assert.That(result[0].Key, Is.EqualTo("Default"));
        Assert.That(result[0].Value, Is.EqualTo(string.Empty));
        Assert.That(result[1].Key, Is.EqualTo("host1.com"));
        Assert.That(result[1].Value, Is.EqualTo("http://host1.com/"));
    }
}
