using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;

using Moq;

using NUnit.Framework;
using Stott.Optimizely.RobotsHandler.Opal;

namespace Stott.Optimizely.RobotsHandler.Test.Opal;

[TestFixture]
public sealed class OpalAuthorizationAttributeTests
{
    private Mock<HttpContext> _mockHttpContext;
    private Mock<HttpRequest> _mockHttpRequest;
    private ActionContext _actionContext;
    private ActionExecutingContext _actionExecutingContext;
    private HeaderDictionary _requestHeaders;

    [SetUp]
    public void Setup()
    {
        

        _requestHeaders = new HeaderDictionary();
        _mockHttpRequest = new Mock<HttpRequest>();
        _mockHttpRequest.Setup(x => x.Headers).Returns(_requestHeaders);

        _mockHttpContext = new Mock<HttpContext>();
        _mockHttpContext.Setup(x => x.Request).Returns(_mockHttpRequest.Object);

        _actionContext = new ActionContext(_mockHttpContext.Object, new RouteData(), new ActionDescriptor(), new ModelStateDictionary());
        _actionExecutingContext = new ActionExecutingContext(_actionContext, new List<IFilterMetadata>(), new Dictionary<string, object>(), null);
    }

    [Test]
    [TestCase(OpalScopeType.Robots, OpalAuthorizationLevel.None)]
    [TestCase(OpalScopeType.Robots, OpalAuthorizationLevel.Read)]
    [TestCase(OpalScopeType.Robots, OpalAuthorizationLevel.Write)]
    [TestCase(OpalScopeType.Llms, OpalAuthorizationLevel.None)]
    [TestCase(OpalScopeType.Llms, OpalAuthorizationLevel.Read)]
    [TestCase(OpalScopeType.Llms, OpalAuthorizationLevel.Write)]
    public void Constructor_GivenValidParameters_SetsPropertiesCorrectly(OpalScopeType opalScopeType, OpalAuthorizationLevel authorizationLevel)
    {
        // Act
        var attribute = new OpalAuthorizationAttribute(opalScopeType, authorizationLevel);

        // Assert
        Assert.That(attribute.ScopeType, Is.EqualTo(opalScopeType));
        Assert.That(attribute.AuthorizationLevel, Is.EqualTo(authorizationLevel));
    }

    [Test]
    public void OnActionExecuting_GivenNoAuthorizationHeader_AndRequiredLevelIsNone_AllowsAccess()
    {
        // Arrange
        var attribute = new OpalAuthorizationAttribute(OpalScopeType.Robots, OpalAuthorizationLevel.None);

        // Act
        attribute.OnActionExecuting(_actionExecutingContext);

        // Assert
        Assert.That(_actionExecutingContext.Result, Is.Null);
    }

    [Test]
    public void OnActionExecuting_GivenNoAuthorizationHeader_AndRequiredLevelIsRead_Returns401()
    {
        // Arrange
        var attribute = new OpalAuthorizationAttribute(OpalScopeType.Robots, OpalAuthorizationLevel.Read);

        // Act
        attribute.OnActionExecuting(_actionExecutingContext);
        var result = _actionExecutingContext.Result as ContentResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(401));
        Assert.That(result.Content, Is.EqualTo("You are not authorized to access this resource."));
        Assert.That(result.ContentType, Is.EqualTo("text/plain"));
    }

    [Test]
    public void OnActionExecuting_GivenNoAuthorizationHeader_AndRequiredLevelIsWrite_Returns401()
    {
        // Arrange
        var attribute = new OpalAuthorizationAttribute(OpalScopeType.Robots, OpalAuthorizationLevel.Write);

        // Act
        attribute.OnActionExecuting(_actionExecutingContext);
        var result = _actionExecutingContext.Result as ContentResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(401));
        Assert.That(result.Content, Is.EqualTo("You are not authorized to access this resource."));
        Assert.That(result.ContentType, Is.EqualTo("text/plain"));
    }

    [Test]
    public void OnActionExecuting_GivenEmptyAuthorizationHeader_Returns401()
    {
        // Arrange
        var attribute = new OpalAuthorizationAttribute(OpalScopeType.Robots, OpalAuthorizationLevel.Read);
        _requestHeaders.Add("Authorization", new StringValues(""));

        // Act
        attribute.OnActionExecuting(_actionExecutingContext);
        var result = (ContentResult)_actionExecutingContext.Result;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(401));
    }

    [Test]
    public void OnActionExecuting_GivenWhitespaceOnlyAuthorizationHeader_Returns401()
    {
        // Arrange
        var attribute = new OpalAuthorizationAttribute(OpalScopeType.Robots, OpalAuthorizationLevel.Read);
        _requestHeaders.Add("Authorization", new StringValues("   "));

        // Act
        attribute.OnActionExecuting(_actionExecutingContext);
        var result = (ContentResult)_actionExecutingContext.Result;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(401));
    }

    [Test]
    public void OnActionExecuting_GivenBearerWithWhitespaceOnly_Returns401()
    {
        // Arrange
        var attribute = new OpalAuthorizationAttribute(OpalScopeType.Robots, OpalAuthorizationLevel.Read);
        _requestHeaders.Add("Authorization", new StringValues("Bearer   "));

        // Act
        attribute.OnActionExecuting(_actionExecutingContext);
        var result = (ContentResult)_actionExecutingContext.Result;

        // Assert
        Assert.That(result, Is.Not.Null); 
        Assert.That(result.StatusCode, Is.EqualTo(401));
    }

    [Test]
    public void OnActionExecuting_GivenInvalidToken_Returns401()
    {
        // Arrange
        var attribute = new OpalAuthorizationAttribute(OpalScopeType.Robots, OpalAuthorizationLevel.Read);
        _requestHeaders.Add("Authorization", new StringValues("Bearer invalid-token-that-does-not-exist"));

        // Act
        attribute.OnActionExecuting(_actionExecutingContext);
        var result = (ContentResult)_actionExecutingContext.Result;

        // Assert
        Assert.That(result, Is.Not.Null); 
        Assert.That(result.StatusCode, Is.EqualTo(401));
    }

    [Test]
    public void OnActionExecuting_GivenRobotsScopeType_SetsCorrectScopeType()
    {
        // Arrange
        var attribute = new OpalAuthorizationAttribute(OpalScopeType.Robots, OpalAuthorizationLevel.None);

        // Act
        attribute.OnActionExecuting(_actionExecutingContext);

        // Assert
        Assert.That(attribute.ScopeType, Is.EqualTo(OpalScopeType.Robots));
        Assert.That(_actionExecutingContext.Result, Is.Null);
    }

    [Test]
    public void OnActionExecuting_GivenLlmsScopeType_SetsCorrectScopeType()
    {
        // Arrange
        var attribute = new OpalAuthorizationAttribute(OpalScopeType.Llms, OpalAuthorizationLevel.None);

        // Act
        attribute.OnActionExecuting(_actionExecutingContext);

        // Assert
        Assert.That(attribute.ScopeType, Is.EqualTo(OpalScopeType.Llms));
        Assert.That(_actionExecutingContext.Result, Is.Null);
    }

    [Test]
    public void OnActionExecuting_GivenAuthorizationLevelNone_WithNoToken_AllowsAccess()
    {
        // Arrange
        var attribute = new OpalAuthorizationAttribute(OpalScopeType.Robots, OpalAuthorizationLevel.None);

        // Act
        attribute.OnActionExecuting(_actionExecutingContext);

        // Assert
        Assert.That(_actionExecutingContext.Result, Is.Null);
    }

    [Test]
    public void OnActionExecuting_WhenServiceLocatorFails_ReturnsNoneAuthorization()
    {
        // Arrange
        var attribute = new OpalAuthorizationAttribute(OpalScopeType.Robots, OpalAuthorizationLevel.Read);
        _requestHeaders.Add("Authorization", new StringValues("Bearer some-token"));

        // Act
        attribute.OnActionExecuting(_actionExecutingContext);
        var result = (ContentResult)_actionExecutingContext.Result;

        // Assert
        Assert.That(result, Is.Not.Null); 
        Assert.That(result.StatusCode, Is.EqualTo(401));
    }
}