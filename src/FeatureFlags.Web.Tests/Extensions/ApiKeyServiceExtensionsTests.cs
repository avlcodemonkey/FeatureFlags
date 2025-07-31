using FeatureFlags.Domain.Models;
using FeatureFlags.Extensions.Services;

namespace FeatureFlags.Web.Tests.Extensions;

public class ApiKeyServiceExtensionsTests {
    [Fact]
    public void SelectAsModel_ProjectsApiKeyToApiKeyModel() {
        // Arrange
        var now = DateTime.UtcNow;
        var apiKey = new ApiKey {
            Id = 42,
            Name = "Test Key",
            Key = "should not be used",
            CreatedDate = now,
            UpdatedDate = now.AddMinutes(1)
        };
        var apiKeys = new List<ApiKey> { apiKey }.AsQueryable();

        // Act
        var models = apiKeys.SelectAsModel().ToList();

        // Assert
        Assert.Single(models);
        var model = models[0];
        Assert.Equal(apiKey.Id, model.Id);
        Assert.Equal(apiKey.Name, model.Name);
        Assert.Equal(new string('x', 32), model.Key); // Key is always masked
        Assert.Equal(apiKey.CreatedDate, model.CreatedDate);
        Assert.Equal(apiKey.UpdatedDate, model.UpdatedDate);
    }

    [Fact]
    public void SelectAsModel_ProjectsMultipleApiKeys() {
        // Arrange
        var now = DateTime.UtcNow;
        var apiKey1 = new ApiKey { Id = 1, Name = "Key1", CreatedDate = now, UpdatedDate = now };
        var apiKey2 = new ApiKey { Id = 2, Name = "Key2", CreatedDate = now.AddDays(-1), UpdatedDate = now.AddDays(-1) };
        var apiKeys = new List<ApiKey> { apiKey1, apiKey2 }.AsQueryable();

        // Act
        var models = apiKeys.SelectAsModel().ToList();

        // Assert
        Assert.Equal(2, models.Count);
        Assert.Collection(models,
            x => {
                Assert.Equal(apiKey1.Id, x.Id);
                Assert.Equal(apiKey1.Name, x.Name);
                Assert.Equal(new string('x', 32), x.Key);
                Assert.Equal(apiKey1.CreatedDate, x.CreatedDate);
                Assert.Equal(apiKey1.UpdatedDate, x.UpdatedDate);
            },
            x => {
                Assert.Equal(apiKey2.Id, x.Id);
                Assert.Equal(apiKey2.Name, x.Name);
                Assert.Equal(new string('x', 32), x.Key);
                Assert.Equal(apiKey2.CreatedDate, x.CreatedDate);
                Assert.Equal(apiKey2.UpdatedDate, x.UpdatedDate);
            }
        );
    }
}
