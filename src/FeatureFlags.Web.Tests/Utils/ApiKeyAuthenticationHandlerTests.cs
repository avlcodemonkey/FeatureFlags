using System.Security.Claims;
using System.Text.Encodings.Web;
using FeatureFlags.Constants;
using FeatureFlags.Models;
using FeatureFlags.Services;
using FeatureFlags.Utils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace FeatureFlags.Web.Tests.Utils;

public class ApiKeyAuthenticationHandlerTests {
    private const string _ApiKeyHeader = "X-Api-Key";
    private const string _ValidApiKey = "valid-key";
    private const string _ApiKeyName = "TestKey";

    private static DefaultHttpContext CreateHttpContext(string? apiKey = null, int headerCount = 1) {
        var context = new DefaultHttpContext();
        if (apiKey != null) {
            for (var i = 0; i < headerCount; i++) {
                context.Request.Headers.Append(_ApiKeyHeader, apiKey);
            }
        }
        return context;
    }

    private static ApiKeyAuthenticationHandler CreateHandler(
        IApiKeyService apiKeyService,
        HttpContext httpContext) {
        var options = new Mock<IOptionsMonitor<ApiKeyAuthenticationOptions>>();
        options.Setup(o => o.Get(It.IsAny<string>())).Returns(new ApiKeyAuthenticationOptions());

        var loggerFactory = new LoggerFactory();
        var encoder = UrlEncoder.Default;

        var handler = new ApiKeyAuthenticationHandler(apiKeyService, options.Object, loggerFactory, encoder);
        handler.InitializeAsync(
            new AuthenticationScheme(ApiKeyAuthenticationOptions.AuthenticationScheme, null, typeof(ApiKeyAuthenticationHandler)),
            httpContext
        ).GetAwaiter().GetResult();

        return handler;
    }

    [Fact]
    public async Task HandleAuthenticateAsync_ReturnsFail_WhenHeaderMissing() {
        var apiKeyService = new Mock<IApiKeyService>();
        var context = CreateHttpContext(null);

        var handler = CreateHandler(apiKeyService.Object, context);

        var result = await handler.AuthenticateAsync();

        Assert.False(result.Succeeded);
        Assert.Equal("Invalid parameters", result.Failure?.Message);
    }

    [Fact]
    public async Task HandleAuthenticateAsync_ReturnsFail_WhenHeaderCountNotOne() {
        var apiKeyService = new Mock<IApiKeyService>();
        var context = CreateHttpContext(_ValidApiKey, headerCount: 2);

        var handler = CreateHandler(apiKeyService.Object, context);

        var result = await handler.AuthenticateAsync();

        Assert.False(result.Succeeded);
        Assert.Equal("Invalid parameters", result.Failure?.Message);
    }

    [Fact]
    public async Task HandleAuthenticateAsync_ReturnsFail_WhenApiKeyIsEmpty() {
        var apiKeyService = new Mock<IApiKeyService>();
        var context = CreateHttpContext("");

        var handler = CreateHandler(apiKeyService.Object, context);

        var result = await handler.AuthenticateAsync();

        Assert.False(result.Succeeded);
        Assert.Equal("Invalid parameters", result.Failure?.Message);
    }

    [Fact]
    public async Task HandleAuthenticateAsync_ReturnsFail_WhenApiKeyNotFound() {
        var apiKeyService = new Mock<IApiKeyService>();
        apiKeyService.Setup(s => s.GetApiKeyByKeyAsync(It.IsAny<string>(), default)).ReturnsAsync((ApiKeyModel?)null);

        var context = CreateHttpContext(_ValidApiKey);

        var handler = CreateHandler(apiKeyService.Object, context);

        var result = await handler.AuthenticateAsync();

        Assert.False(result.Succeeded);
        Assert.Equal("Invalid parameters", result.Failure?.Message);
    }

    [Fact]
    public async Task HandleAuthenticateAsync_ReturnsSuccess_WhenApiKeyIsValid() {
        var apiKeyService = new Mock<IApiKeyService>();
        apiKeyService.Setup(s => s.GetApiKeyByKeyAsync(_ValidApiKey, default))
            .ReturnsAsync(new ApiKeyModel { Name = _ApiKeyName });

        var context = CreateHttpContext(_ValidApiKey);

        var handler = CreateHandler(apiKeyService.Object, context);

        var result = await handler.AuthenticateAsync();

        Assert.True(result.Succeeded);
        var principal = result.Principal;
        Assert.NotNull(principal);
        var identity = Assert.IsType<ClaimsIdentity>(principal.Identity);
        Assert.Equal(ApiKeyAuthenticationOptions.AuthenticationScheme, identity.AuthenticationType);
        Assert.Equal(_ApiKeyName, identity.Name);
    }
}
