using FeatureFlags.Client;
using FeatureFlags.Controllers;
using FeatureFlags.Models;
using FeatureFlags.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace FeatureFlags.Web.Tests.Controllers;

public class FeatureFlagApiControllerTests {
    private readonly Mock<IFeatureFlagService> _MockFeatureFlagService = new();
    private readonly Mock<IApiKeyService> _MockApiKeyService = new();
    private readonly Mock<IApiRequestService> _MockApiRequestService = new();
    private readonly Mock<ILogger<FeatureFlagApiController>> _MockLogger = new();

    private const string _ApiKey = "test-api-key";

    private FeatureFlagApiController CreateController() => new(_MockFeatureFlagService.Object, _MockApiKeyService.Object, _MockApiRequestService.Object, _MockLogger.Object) {
        ControllerContext = new ControllerContext {
            HttpContext = new DefaultHttpContext()
        }
    };

    [Fact]
    public async Task GetAllFeatureDefinitionsAsync_ReturnsOkWithDefinitions() {
        // Arrange
        var flags = new List<FeatureFlagModel>
        {
            new() { Id = 1, Name = "Flag1", Status = true },
            new() { Id = 2, Name = "Flag2", Status = true }
        };
        _MockApiKeyService.Setup(x => x.GetApiKeyByKeyAsync(_ApiKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiKeyModel { Id = 1, Key = _ApiKey, Name = "Test Key" });
        _MockFeatureFlagService.Setup(s => s.GetAllFeatureFlagsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(flags);
        var controller = CreateController();

        // Act
        var result = await controller.GetAllFeatureDefinitionsAsync(_ApiKey);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var definitions = Assert.IsType<IEnumerable<CustomFeatureDefinition>>(okResult.Value, exactMatch: false);
        Assert.Equal(2, definitions.Count());
        Assert.Equal("Flag1", definitions.First().Name);
        Assert.NotNull(definitions.First().EnabledFor);
        Assert.Equal("Flag2", definitions.Last().Name);
        Assert.NotNull(definitions.Last().EnabledFor);
    }

    [Fact]
    public async Task GetAllFeatureDefinitionsAsync_WithDisabled_ReturnsOkWithDefinitions() {
        // Arrange
        var flags = new List<FeatureFlagModel>
        {
            new() { Id = 1, Name = "Flag1", Status = false },
            new() { Id = 2, Name = "Flag2", Status = true }
        };
        _MockApiKeyService.Setup(x => x.GetApiKeyByKeyAsync(_ApiKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiKeyModel { Id = 1, Key = _ApiKey, Name = "Test Key" });
        _MockFeatureFlagService.Setup(s => s.GetAllFeatureFlagsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(flags);
        var controller = CreateController();

        // Act
        var result = await controller.GetAllFeatureDefinitionsAsync(_ApiKey);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var definitions = Assert.IsType<IEnumerable<CustomFeatureDefinition>>(okResult.Value, exactMatch: false);
        Assert.NotNull(definitions);
        Assert.Equal(2, definitions.Count());
    }

    [Fact]
    public async Task GetAllFeatureDefinitionsAsync_ReturnsOkWithEmptyList() {
        // Arrange
        _MockApiKeyService.Setup(x => x.GetApiKeyByKeyAsync(_ApiKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiKeyModel { Id = 1, Key = _ApiKey, Name = "Test Key" });
        _MockFeatureFlagService.Setup(s => s.GetAllFeatureFlagsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FeatureFlagModel>());
        var controller = CreateController();

        // Act
        var result = await controller.GetAllFeatureDefinitionsAsync(_ApiKey);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var definitions = Assert.IsType<IEnumerable<CustomFeatureDefinition>>(okResult.Value, exactMatch: false);
        Assert.Empty(definitions);
    }

    [Fact]
    public async Task GetAllFeatureDefinitionsAsync_ReturnsUnauthorized_WhenApiKeyHeaderIsMissing() {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.GetAllFeatureDefinitionsAsync(null!);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task GetAllFeatureDefinitionsAsync_ReturnsUnauthorized_WhenApiKeyIsInvalid() {
        // Arrange
        _MockApiKeyService.Setup(x => x.GetApiKeyByKeyAsync(_ApiKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApiKeyModel?)null);
        var controller = CreateController();

        // Act
        var result = await controller.GetAllFeatureDefinitionsAsync(_ApiKey);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task GetAllFeatureDefinitionsAsync_LogsError_WhenApiRequestSaveFails() {
        // Arrange
        var flags = new List<FeatureFlagModel>
        {
            new() { Id = 1, Name = "Flag1", Status = true }
        };
        _MockApiKeyService.Setup(x => x.GetApiKeyByKeyAsync(_ApiKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiKeyModel { Id = 1, Key = _ApiKey, Name = "Test Key" });
        _MockFeatureFlagService.Setup(s => s.GetAllFeatureFlagsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(flags);
        _MockApiRequestService.Setup(x => x.SaveApiRequestAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));
        var controller = CreateController();

        // Act
        var result = await controller.GetAllFeatureDefinitionsAsync(_ApiKey);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var definitions = Assert.IsType<IEnumerable<CustomFeatureDefinition>>(okResult.Value, false);
        Assert.Single(definitions);

        // Verify logging
        _MockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to log API request for API key ID")),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAllFeatureDefinitionsAsync_ReturnsOk_WhenRemoteIpAddressIsNull() {
        // Arrange
        var flags = new List<FeatureFlagModel>
        {
            new() { Id = 1, Name = "Flag1", Status = true }
        };
        _MockApiKeyService.Setup(x => x.GetApiKeyByKeyAsync(_ApiKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiKeyModel { Id = 1, Key = _ApiKey, Name = "Test Key" });
        _MockFeatureFlagService.Setup(s => s.GetAllFeatureFlagsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(flags);
        var controller = CreateController();
        controller.ControllerContext.HttpContext.Connection.RemoteIpAddress = null;

        // Act
        var result = await controller.GetAllFeatureDefinitionsAsync(_ApiKey);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var definitions = Assert.IsType<IEnumerable<CustomFeatureDefinition>>(okResult.Value, false);
        Assert.Single(definitions);
    }

    [Fact]
    public async Task GetAllFeatureDefinitionsAsync_UsesXForwardedForHeader_WhenProvided() {
        // Arrange
        var flags = new List<FeatureFlagModel>
        {
            new() { Id = 1, Name = "Flag1", Status = true }
        };
        _MockApiKeyService.Setup(x => x.GetApiKeyByKeyAsync(_ApiKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiKeyModel { Id = 1, Key = _ApiKey, Name = "Test Key" });
        _MockFeatureFlagService.Setup(s => s.GetAllFeatureFlagsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(flags);
        var controller = CreateController();

        // Act
        var result = await controller.GetAllFeatureDefinitionsAsync(_ApiKey, "192.168.1.1");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var definitions = Assert.IsType<IEnumerable<CustomFeatureDefinition>>(okResult.Value, false);
        Assert.Single(definitions);
        _MockApiRequestService.Verify(x => x.SaveApiRequestAsync(1, "192.168.1.1", It.IsAny<CancellationToken>()), Times.Once);
    }
}
