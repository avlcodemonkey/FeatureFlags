using FeatureFlags.Models;
using FeatureFlags.Services;
using FeatureFlags.Utils;
using Moq;

namespace FeatureFlags.Web.Tests.Utils;

public class DbFeatureDefinitionProviderTests {
    private readonly Mock<IFeatureFlagService> _FeatureFlagServiceMock;
    private readonly DbFeatureDefinitionProvider _Provider;

    public DbFeatureDefinitionProviderTests() {
        _FeatureFlagServiceMock = new Mock<IFeatureFlagService>();
        _Provider = new DbFeatureDefinitionProvider(_FeatureFlagServiceMock.Object);
    }

    [Fact]
    public void GetAllFeatureDefinitionsAsync_ReturnsAllFeatureDefinitions() {
        // Arrange
        var featureFlags = new List<FeatureFlagModel>
        {
            new() { Name = "Feature1", IsEnabled = true },
            new() { Name = "Feature2", IsEnabled = false }
        };
        _FeatureFlagServiceMock.Setup(s => s.GetAllFeatureFlagsAsync(default)).ReturnsAsync(featureFlags);

        // Act
        var result = _Provider.GetAllFeatureDefinitionsAsync();

        // Assert
        var definitions = result.ToBlockingEnumerable().ToList();
        Assert.Equal(2, definitions.Count);
        Assert.Equal("Feature1", definitions[0].Name);
        Assert.NotNull(definitions[0].EnabledFor);
        Assert.Equal("Feature2", definitions[1].Name);
        Assert.Null(definitions[1].EnabledFor);
    }

    [Fact]
    public async Task GetFeatureDefinitionAsync_ReturnsFeatureDefinition_WhenFeatureExists() {
        // Arrange
        var featureFlag = new FeatureFlagModel { Name = "Feature1", IsEnabled = true };
        var featureFlags = new List<FeatureFlagModel> { featureFlag };
        _FeatureFlagServiceMock.Setup(s => s.GetAllFeatureFlagsAsync(default)).ReturnsAsync(featureFlags);

        // Act
        var result = await _Provider.GetFeatureDefinitionAsync("Feature1");

        // Assert
        Assert.Equal("Feature1", result.Name);
        Assert.NotNull(result.EnabledFor);
    }

    [Fact]
    public async Task GetFeatureDefinitionAsync_ReturnsEmptyFeatureDefinition_WhenFeatureDoesNotExist() {
        // Arrange
        var featureFlags = new List<FeatureFlagModel>();
        _FeatureFlagServiceMock.Setup(s => s.GetAllFeatureFlagsAsync(default)).ReturnsAsync(featureFlags);

        // Act
        var result = await _Provider.GetFeatureDefinitionAsync("NonExistentFeature");

        // Assert
        Assert.Equal("NonExistentFeature", result.Name);
        Assert.Empty(result.EnabledFor);
    }
}
