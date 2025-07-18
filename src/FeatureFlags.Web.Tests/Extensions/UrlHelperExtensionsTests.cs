using FeatureFlags.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Moq;

namespace FeatureFlags.Web.Tests.Extensions;

public class UrlHelperExtensionsTests {
    private readonly Mock<IUrlHelper> _MockUrlHelper = new();

    [Fact]
    public void ActionForMustache_ReturnsTemplatedUrl() {
        // arrange
        _MockUrlHelper.Setup(x => x.Action(It.IsAny<UrlActionContext>())).Returns("/test");

        // act
        var url = _MockUrlHelper.Object.ActionForMustache("test");

        // assert
        Assert.Equal("/test/{{id}}", url);
    }

    [Theory]
    [InlineData("example.com", "/path", "https://example.com/path")]
    [InlineData("www.example.com", "/path", "https://www.example.com/path")]
    [InlineData("example.com", "~/random/dir", "https://example.com/random/dir")]
    [InlineData("www.example.com", "~/random/dir", "https://www.example.com/random/dir")]
    [InlineData("example.com", "http://www.google.com/test", "http://www.google.com/test")]
    [InlineData("www.example.com", "http://www.google.com/test", "http://www.google.com/test")]
    [InlineData("example.com", "https://www.google.com/test", "https://www.google.com/test")]
    [InlineData("www.example.com", "https://www.google.com/test", "https://www.google.com/test")]
    public void ConvertToAbsolute_ReturnsExpectedValue(string host, string url, string expectedUrl) {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = "GET";
        httpContext.Request.Scheme = "https";
        httpContext.Request.Host = new HostString(host);
        httpContext.Request.ContentType = "application/json";

        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var urlHelper = new UrlHelper(actionContext);

        // Act
        var result = urlHelper.ConvertToAbsolute(url);

        // Assert
        Assert.Equal(expectedUrl, result);
    }
}
