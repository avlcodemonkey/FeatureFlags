using FeatureFlags.Models;
using FeatureFlags.Services;
using FeatureFlags.Utils;
using Microsoft.FeatureManagement;
using Moq;

namespace FeatureFlags.Web.Tests.Utils;

public class InternalFeatureDefinitionProviderTests {
    [Fact]
    public async Task GetAllFeatureDefinitionsAsync_ReturnsMappedDefinitions() {
        // Arrange
        var flags = new[]
        {
            new FeatureFlagModel { Name = "FlagA" },
            new FeatureFlagModel { Name = "FlagB" }
        };

        var serviceMock = new Mock<IFeatureFlagService>();
        serviceMock.Setup(s => s.GetAllFeatureFlagsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(flags);

        var provider = new InternalFeatureDefinitionProvider(serviceMock.Object);

        // Act
        var results = new List<FeatureDefinition>();
        await foreach (var def in provider.GetAllFeatureDefinitionsAsync()) {
            results.Add(def);
        }

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Contains(results, d => d.Name == "FlagA");
        Assert.Contains(results, d => d.Name == "FlagB");
    }

    [Fact]
    public async Task GetFeatureDefinitionAsync_ReturnsMappedDefinition_WhenFlagExists() {
        // Arrange
        var flag = new FeatureFlagModel { Name = "FlagX" };
        var serviceMock = new Mock<IFeatureFlagService>();
        serviceMock.Setup(s => s.GetFeatureFlagByNameAsync("FlagX", It.IsAny<CancellationToken>())).ReturnsAsync(flag);

        var provider = new InternalFeatureDefinitionProvider(serviceMock.Object);

        // Act
        var result = await provider.GetFeatureDefinitionAsync("FlagX");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("FlagX", result.Name);
    }

    [Fact]
    public async Task GetFeatureDefinitionAsync_ReturnsEmptyDefinition_WhenFlagNotFound() {
        // Arrange
        var serviceMock = new Mock<IFeatureFlagService>();
        serviceMock.Setup(s => s.GetFeatureFlagByNameAsync("MissingFlag", It.IsAny<CancellationToken>())).ReturnsAsync((FeatureFlagModel?)null);

        var provider = new InternalFeatureDefinitionProvider(serviceMock.Object);

        // Act
        var result = await provider.GetFeatureDefinitionAsync("MissingFlag");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("MissingFlag", result.Name);
    }
}
