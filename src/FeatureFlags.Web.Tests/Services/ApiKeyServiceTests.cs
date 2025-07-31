using FeatureFlags.Domain.Models;
using FeatureFlags.Models;
using FeatureFlags.Resources;
using FeatureFlags.Services;
using FeatureFlags.Web.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace FeatureFlags.Web.Tests.Services;

[Collection(nameof(DatabaseCollection))]
public class ApiKeyServiceTests {
    private readonly DatabaseFixture _fixture;
    private readonly ApiKeyService _ApiKeyService;

    public ApiKeyServiceTests(DatabaseFixture fixture) {
        _fixture = fixture;
        _ApiKeyService = CreateService();
    }

    private ApiKeyService CreateService() => new(_fixture.CreateContext());

    [Fact]
    public async Task GetAllApiKeysAsync_ReturnsAllApiKeys() {
        // Arrange
        var context = _fixture.CreateContext();
        var now = DateTime.UtcNow;
        context.ApiKeys.Add(new ApiKey { Name = "getAll1", Key = "hash1", CreatedDate = now, UpdatedDate = now });
        context.ApiKeys.Add(new ApiKey { Name = "getAll2", Key = "hash2", CreatedDate = now, UpdatedDate = now });
        await context.SaveChangesAsync();

        // Act
        var result = await _ApiKeyService.GetAllApiKeysAsync();

        // Assert
        Assert.True(result.Count() >= 2);
        Assert.All(result, x => Assert.Equal(new string('x', 32), x.Key));
    }

    [Fact]
    public async Task GetApiKeyByKeyAsync_ReturnsApiKeyModel_WhenKeyMatches() {
        // Arrange
        var context = _fixture.CreateContext();
        var plainKey = "my-secret";
        var hashedKey = FeatureFlags.Utils.KeyGenerator.GetSha512Hash(plainKey);
        var now = DateTime.UtcNow;
        var apiKey = new ApiKey { Name = "getKey1", Key = hashedKey, CreatedDate = now, UpdatedDate = now };
        context.ApiKeys.Add(apiKey);
        await context.SaveChangesAsync();

        // Act
        var result = await _ApiKeyService.GetApiKeyByKeyAsync(plainKey);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(apiKey.Name, result.Name);
        Assert.Equal(new string('x', 32), result.Key);
    }

    [Fact]
    public async Task GetApiKeyByKeyAsync_ReturnsNull_WhenKeyDoesNotMatch() {
        // Arrange
        var context = _fixture.CreateContext();
        context.ApiKeys.Add(new ApiKey { Name = "nullKey1", Key = "somehash", CreatedDate = DateTime.UtcNow });
        await context.SaveChangesAsync();

        // Act
        var result = await _ApiKeyService.GetApiKeyByKeyAsync("not-a-match");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SaveApiKeyAsync_SuccessfullySavesNewApiKey() {
        // Arrange
        var model = new ApiKeyModel { Name = "UniqueName", Key = "plain-key" };

        // Act
        var (success, message) = await _ApiKeyService.SaveApiKeyAsync(model);

        // Assert
        Assert.True(success);
        Assert.Equal(ApiKeys.SuccessSavingApiKey, message);

        // Confirm in DB
        using var context = _fixture.CreateContext();
        var saved = await context.ApiKeys.FirstOrDefaultAsync(x => x.Name == "UniqueName");
        Assert.NotNull(saved);
        Assert.Equal(FeatureFlags.Utils.KeyGenerator.GetSha512Hash("plain-key"), saved.Key);
    }

    [Fact]
    public async Task SaveApiKeyAsync_ReturnsError_WhenNameIsDuplicate() {
        // Arrange
        var context = _fixture.CreateContext();
        context.ApiKeys.Add(new ApiKey { Name = "Duplicate", Key = "hash" });
        await context.SaveChangesAsync();

        var model = new ApiKeyModel { Name = "Duplicate", Key = "another-key" };

        // Act
        var (success, message) = await _ApiKeyService.SaveApiKeyAsync(model);

        // Assert
        Assert.False(success);
        Assert.Equal(ApiKeys.ErrorDuplicateName, message);
    }

    [Fact]
    public async Task SaveApiKeyAsync_Throws_WhenUpdatingApiKey() {
        // Arrange
        var model = new ApiKeyModel { Id = 99, Name = "Update", Key = "key" };

        // Act & Assert
        await Assert.ThrowsAsync<NotImplementedException>(() => _ApiKeyService.SaveApiKeyAsync(model));
    }

    [Fact]
    public async Task DeleteApiKeyAsync_DeletesAndReturnsTrue_WhenIdExists() {
        // Arrange
        var context = _fixture.CreateContext();
        var apiKey = new ApiKey { Name = "deleteKey1", Key = "hash" };
        context.ApiKeys.Add(apiKey);
        await context.SaveChangesAsync();

        // Act
        var result = await _ApiKeyService.DeleteApiKeyAsync(apiKey.Id);

        // Assert
        Assert.True(result);
        using var checkContext = _fixture.CreateContext();
        Assert.Null(await checkContext.ApiKeys.FindAsync(apiKey.Id));
    }

    [Fact]
    public async Task DeleteApiKeyAsync_ReturnsFalse_WhenIdDoesNotExist() {
        // Arrange
        // Act
        var result = await _ApiKeyService.DeleteApiKeyAsync(-999);

        // Assert
        Assert.False(result);
    }
}
