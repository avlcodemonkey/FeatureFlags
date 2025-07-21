using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.FeatureManagement;
using Moq;

namespace FeatureFlags.Client.Tests;

public class ExtensionsTests {
    [Fact]
    public void AddFeatureFlags_ThrowsIfApiBaseEndpointMissing() {
        // Arrange
        var configurationManager = new ConfigurationManager();
        configurationManager.AddInMemoryCollection(new Dictionary<string, string?> {
            { "FeatureFlags:ApiBaseEndpoint", null },
            { "FeatureFlags:ApiKey", "valid-key" }
        });
        var builderMock = new Mock<IHostApplicationBuilder>();
        builderMock.SetupGet(b => b.Configuration).Returns(configurationManager);

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => Extensions.AddFeatureFlags(builderMock.Object));
        Assert.Contains("ApiBaseEndpoint", ex.Message);
    }

    [Fact]
    public void AddFeatureFlags_ThrowsIfApiKeyMissing() {
        // Arrange
        var configurationManager = new ConfigurationManager();
        configurationManager.AddInMemoryCollection(new Dictionary<string, string?> {
            { "FeatureFlags:ApiBaseEndpoint", "https://api.example.com" },
            { "FeatureFlags:ApiKey", null }
        });
        var builderMock = new Mock<IHostApplicationBuilder>();
        builderMock.SetupGet(b => b.Configuration).Returns(configurationManager);

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => Extensions.AddFeatureFlags(builderMock.Object));
        Assert.Contains("ApiKey", ex.Message);
    }

    [Fact]
    public void AddFeatureFlags_RegistersServicesAndReturnsBuilder() {
        // Arrange
        var configurationManager = new ConfigurationManager();
        configurationManager.AddInMemoryCollection(new Dictionary<string, string?> {
            { "FeatureFlags:ApiBaseEndpoint", "https://api.example.com/" },
            { "FeatureFlags:ApiKey", "valid-key" }
        });
        var builderMock = new Mock<IHostApplicationBuilder>();
        builderMock.SetupGet(b => b.Configuration).Returns(configurationManager);
        var services = new ServiceCollection();
        builderMock.SetupGet(b => b.Services).Returns(services);

        // Act
        var result = Extensions.AddFeatureFlags(builderMock.Object);

        // Assert
        Assert.Same(builderMock.Object, result);
        Assert.Contains(services, s => s.ServiceType == typeof(IFeatureFlagClient));
        Assert.Contains(services, s => s.ServiceType == typeof(IFeatureDefinitionProvider));
        Assert.Contains(services, s => s.ServiceType == typeof(IMemoryCache));
        Assert.Contains(services, s => s.ServiceType == typeof(IHttpClientFactory));

        // Build the service provider and get the factory
        var provider = services.BuildServiceProvider();
        var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
        var client = httpClientFactory.CreateClient(Constants.HttpClientName);

        // Assert
        Assert.Equal(new Uri("https://api.example.com/"), client.BaseAddress);
        Assert.NotNull(client.DefaultRequestHeaders.Authorization);
        Assert.Equal("Bearer", client.DefaultRequestHeaders.Authorization.Scheme);
        Assert.Equal("valid-key", client.DefaultRequestHeaders.Authorization.Parameter);
    }

    [Fact]
    public void ToFeatureDefinition_ReturnsDefinition() {
        // arrange
        var customFeatureDefinition = new CustomFeatureDefinition {
            Name = "feature name",
            EnabledFor = [new CustomFeatureFilterConfiguration { Name = "filter name" }],
            Variants = [new VariantDefinition { Name = "variant name" }],
        };

        // act
        var definition = customFeatureDefinition.ToFeatureDefinition();

        // assert
        Assert.NotNull(definition);
        Assert.Equal(customFeatureDefinition.Name, definition.Name);
        Assert.NotNull(definition.EnabledFor);
        Assert.Single(definition.EnabledFor);
        Assert.Equal(definition.EnabledFor.First().Name, customFeatureDefinition.EnabledFor.First().Name);
        Assert.NotNull(definition.Variants);
        Assert.Single(definition.Variants);
        Assert.Equal(definition.Variants.First().Name, customFeatureDefinition.Variants.First().Name);
    }

    [Fact]
    public void ToFeatureDefinition_WithEmptyEnabledFor_ReturnsDefinition() {
        // arrange
        var customFeatureDefinition = new CustomFeatureDefinition {
            Name = "feature name"
        };

        // act
        var definition = customFeatureDefinition.ToFeatureDefinition();

        // assert
        Assert.NotNull(definition);
        Assert.Equal(customFeatureDefinition.Name, definition.Name);
        Assert.NotNull(definition.EnabledFor);
        Assert.Empty(definition.EnabledFor);
    }

    [Fact]
    public void ToFeatureDefinition_WithEmptyVariants_ReturnsDefinition() {
        // arrange
        var customFeatureDefinition = new CustomFeatureDefinition {
            Name = "feature name"
        };

        // act
        var definition = customFeatureDefinition.ToFeatureDefinition();

        // assert
        Assert.NotNull(definition);
        Assert.Equal(customFeatureDefinition.Name, definition.Name);
        Assert.NotNull(definition.Variants);
        Assert.Empty(definition.Variants);
    }
}
