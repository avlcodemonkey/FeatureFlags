using FeatureFlags.Controllers;
using FeatureFlags.Models;
using FeatureFlags.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Moq;

namespace FeatureFlags.Web.Tests.Controllers;

public class FeatureFlagApiControllerTests {
    private readonly Mock<IFeatureFlagService> _MockFeatureFlagService = new();
    private readonly Mock<ILogger<FeatureFlagApiController>> _MockLogger = new();

    private FeatureFlagApiController CreateController() => new(_MockFeatureFlagService.Object, _MockLogger.Object) {
        ControllerContext = new ControllerContext {
            HttpContext = new DefaultHttpContext()
        }
    };

    [Fact]
    public async Task GetAllFeatureDefinitionsAsync_ReturnsOkWithDefinitions() {
        // Arrange
        var flags = new List<FeatureFlagModel>
        {
            new() { Id = 1, Name = "Flag1", IsEnabled = true },
            new() { Id = 2, Name = "Flag2", IsEnabled = false }
        };
        _MockFeatureFlagService.Setup(s => s.GetAllFeatureFlagsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(flags);
        var controller = CreateController();

        // Act
        var result = await controller.GetAllFeatureDefinitionsAsync();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var definitions = Assert.IsType<IEnumerable<FeatureDefinition>>(okResult.Value, exactMatch: false);
        Assert.Equal(2, definitions.Count());
        Assert.Equal("Flag1", definitions.First().Name);
        Assert.NotNull(definitions.First().EnabledFor);
        Assert.Equal("Flag2", definitions.Last().Name);
        Assert.Null(definitions.Last().EnabledFor);
    }

    [Fact]
    public async Task GetAllFeatureDefinitionsAsync_ReturnsOkWithEmptyList() {
        // Arrange
        _MockFeatureFlagService.Setup(s => s.GetAllFeatureFlagsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FeatureFlagModel>());
        var controller = CreateController();

        // Act
        var result = await controller.GetAllFeatureDefinitionsAsync();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var definitions = Assert.IsAssignableFrom<IEnumerable<FeatureDefinition>>(okResult.Value);
        Assert.Empty(definitions);
    }
}
