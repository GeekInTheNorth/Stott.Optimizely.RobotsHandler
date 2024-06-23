using System;

using Microsoft.AspNetCore.Http;

using NUnit.Framework;

using Stott.Optimizely.RobotsHandler.Extensions;
using Stott.Optimizely.RobotsHandler.Test.TestCases;

namespace Stott.Optimizely.RobotsHandler.Test.Extensions;

[TestFixture]
public sealed class HeaderDictionaryExtensionTests
{
    [Test]
    public void AddOrUpdateHeader_WhenCalledWithNullHeaders_DoesNotThrowException()
    {
        // Arrange
        IHeaderDictionary headers = null;
        var headerName = "headerName";
        var headerValue = "headerValue";

        // Act
        Assert.DoesNotThrow(() => headers.AddOrUpdateHeader(headerName, headerValue));
    }

    [Test]
    [TestCaseSource(typeof(CommonTestCases), nameof(CommonTestCases.EmptyStrings))]
    public void AddOrUpdateHeader_WhenCalledWithEmptyHeaderName_DoesNotThrowException(string headerName)
    {
        // Arrange
        var headers = new HeaderDictionary();
        var headerValue = "headerValue";

        // Act
        Assert.DoesNotThrow(() => headers.AddOrUpdateHeader(headerName, headerValue));
    }

    [Test]
    [TestCaseSource(typeof(CommonTestCases), nameof(CommonTestCases.EmptyStrings))]
    public void AddOrUpdateHeader_WhenCalledWithEmptyHeaderValue_DoesNotThrowException(string headerValue)
    {
        // Arrange
        var headers = new HeaderDictionary();
        var headerName = "headerName";

        // Act
        Assert.DoesNotThrow(() => headers.AddOrUpdateHeader(headerName, headerValue));
    }

    [Test]
    [TestCaseSource(typeof(CommonTestCases), nameof(CommonTestCases.EmptyStrings))]
    public void AddOrUpdateHeader_GivenHeadersContainsHeaderName_WhenGivenAnEmptyHeaderValue_ThenHeaderIsRemoved(string headerValue)
    {
        // Arrange
        var headers = new HeaderDictionary
        {
            { "headerName", "headerValue" }
        };
        var headerName = "headerName";

        // Act
        headers.AddOrUpdateHeader(headerName, string.Empty);

        // Assert
        Assert.That(headers.ContainsKey(headerName), Is.False);
    }

    [Test]
    public void AddOrUpdateHeader_WhenCalledWithExistingHeader_UpdatesHeader()
    {
        // Arrange
        var headers = new HeaderDictionary
        {
            { "headerName", "oldValue" }
        };
        var headerName = "headerName";
        var headerValue = Guid.NewGuid().ToString();

        // Act
        headers.AddOrUpdateHeader(headerName, headerValue);

        // Assert
        Assert.That(headers[headerName], Is.EqualTo(headerValue));
    }

    [Test]
    public void AddOrUpdateHeader_WhenCalledWithNonExistingHeader_AddsHeader()
    {
        // Arrange
        var headers = new HeaderDictionary();
        var headerName = "headerName";
        var headerValue = Guid.NewGuid().ToString();

        // Act
        headers.AddOrUpdateHeader(headerName, headerValue);

        // Assert
        Assert.That(headers[headerName], Is.EqualTo(headerValue));
    }
}