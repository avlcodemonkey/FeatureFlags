using FeatureFlags.Domain.Models;
using FeatureFlags.Models;
using FeatureFlags.Resources;
using FeatureFlags.Services;
using FeatureFlags.Web.Tests.Fixtures;
using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace FeatureFlags.Web.Tests.Services;

[Collection(nameof(DatabaseCollection))]
public class FeatureFlagServiceTests {
    private readonly DatabaseFixture _Fixture;
    private readonly Mock<IMemoryCache> _MockMemoryCache;
    private readonly FeatureFlagService _FeatureFlagService;

    private FeatureFlagService GetNewFeatureFlagService() => new(_Fixture.CreateContext(), _MockMemoryCache.Object);

    /// <summary>
    /// Delete a feature flag so it doesn't interfere with other tests.
    /// </summary>
    private async Task DeleteFeatureFlagIfExistsAsync(int featureFlagId) {
        var featureFlag = (await GetNewFeatureFlagService().GetAllFeatureFlagsAsync()).FirstOrDefault(x => x.Id == featureFlagId);
        if (featureFlag != null) {
            // need a new context for this to avoid concurrency error
            await GetNewFeatureFlagService().DeleteFeatureFlagAsync(featureFlag.Id);
        }
    }

    public FeatureFlagServiceTests(DatabaseFixture fixture) {
        _Fixture = fixture;

        _MockMemoryCache = new Mock<IMemoryCache>();

        _FeatureFlagService = GetNewFeatureFlagService();
    }

    [Fact]
    public async Task GetAllFeatureFlagsAsync_ReturnsFeatureFlagModels() {
        // arrange
        var testFeatureFlag = _Fixture.TestFeatureFlag;

        // act
        var featureFlags = await _FeatureFlagService.GetAllFeatureFlagsAsync();

        // assert
        Assert.NotEmpty(featureFlags);
        Assert.IsType<IEnumerable<FeatureFlagModel>>(featureFlags, exactMatch: false);
        Assert.Contains(featureFlags, x => x.Id == testFeatureFlag.Id);
    }


    [Fact]
    public async Task GetFeatureFlagByIdAsync_ReturnsTestFeatureFlagModel() {
        // arrange
        var testFeatureFlag = _Fixture.TestFeatureFlag;

        // act
        var featureFlag = await _FeatureFlagService.GetFeatureFlagByIdAsync(testFeatureFlag.Id);

        // assert
        Assert.NotNull(featureFlag);
        Assert.IsType<FeatureFlagModel>(featureFlag);
        Assert.Equal(testFeatureFlag.Name, featureFlag.Name);
        Assert.Equal(testFeatureFlag.IsEnabled, featureFlag.IsEnabled);
    }

    [Fact]
    public async Task GetFeatureFlagByIdAsync_WithInvalidFeatureFlagId_ReturnsNull() {
        // arrange
        var featureFlagIdToGet = -200;

        // act
        var result = await _FeatureFlagService.GetFeatureFlagByIdAsync(featureFlagIdToGet);

        // assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetCachedFeatureFlagsAsync_ReturnsFlagsFromCache() {
        // arrange
        var testFeatureFlag = _Fixture.TestFeatureFlag;
        var cacheEntry = new MockCacheEntry { Key = "" };
        var lifeTime = TimeSpan.FromMinutes(Constants.FeatureFlagConstants.CacheLifeTime);
        _MockMemoryCache.Setup(m => m.CreateEntry(It.IsAny<object>())).Returns(cacheEntry);

        // act
        var result = await _FeatureFlagService.GetCachedFeatureFlagsAsync();

        // assert
        Assert.NotNull(result);
        Assert.IsType<IEnumerable<FeatureFlagModel>>(result, exactMatch: false);
        Assert.NotEmpty(result);
        Assert.Contains(result, x => x.Id == testFeatureFlag.Id);
        _MockMemoryCache.Verify(m => m.CreateEntry(It.IsAny<object>()), Times.Once);
        Assert.Equal(lifeTime, cacheEntry.AbsoluteExpirationRelativeToNow);
    }

    [Fact]
    public async Task SaveFeatureFlagAsync_CreateFeatureFlag_SavesNewFeatureFlag() {
        // arrange
        var createFeatureFlag = new FeatureFlagModel {
            Name = "create", IsEnabled = true
        };

        // act
        (var success, var message) = await _FeatureFlagService.SaveFeatureFlagAsync(createFeatureFlag);
        var newFeatureFlag = (await _FeatureFlagService.GetAllFeatureFlagsAsync()).FirstOrDefault(x => x.Name == createFeatureFlag.Name);
        if (newFeatureFlag != null) {
            // delete the newly created flag so it doesn't interfere with other tests
            await GetNewFeatureFlagService().DeleteFeatureFlagAsync(newFeatureFlag.Id);
        }

        // assert
        Assert.True(success);
        Assert.Equal(Flags.SuccessSavingFlag, message);
        Assert.NotNull(newFeatureFlag);
        Assert.Equal(createFeatureFlag.Name, newFeatureFlag.Name);
        Assert.Equal(createFeatureFlag.IsEnabled, newFeatureFlag.IsEnabled);
    }

    [Fact]
    public async Task SaveFeatureFlagAsync_UpdateFeatureFlag_SavesChanges() {
        // arrange
        var originalName = "original";
        var createFeatureFlag = new FeatureFlagModel {
            Name = originalName, IsEnabled = true, UpdatedDate = DateTime.UtcNow
        };
        await _FeatureFlagService.SaveFeatureFlagAsync(createFeatureFlag);

        var featureFlagId = (await _FeatureFlagService.GetAllFeatureFlagsAsync()).FirstOrDefault(x => x.Name == originalName)!.Id;
        var newName = "newName";
        var updateFeatureFlag = new FeatureFlagModel {
            Id = featureFlagId, Name = newName, IsEnabled = false, UpdatedDate = DateTime.UtcNow
        };

        // act
        (var success, var message) = await _FeatureFlagService.SaveFeatureFlagAsync(updateFeatureFlag);
        var updatedFeatureFlag = (await _FeatureFlagService.GetAllFeatureFlagsAsync()).FirstOrDefault(x => x.Id == featureFlagId);
        if (updatedFeatureFlag != null) {
            // delete the newly created featureFlag so it doesn't interfere with other tests
            await GetNewFeatureFlagService().DeleteFeatureFlagAsync(updatedFeatureFlag.Id);
        }

        // assert
        Assert.True(success);
        Assert.Equal(Flags.SuccessSavingFlag, message);
        Assert.NotNull(updatedFeatureFlag);
        Assert.Equal(newName, updatedFeatureFlag.Name);
        Assert.False(updatedFeatureFlag.IsEnabled);
    }

    [Fact]
    public async Task SaveFeatureFlagAsync_UpdateInvalidFeatureFlagId_ReturnsFalse() {
        // arrange
        var updateFeatureFlag = new FeatureFlagModel {
            Id = 999, Name = "update", IsEnabled = true
        };

        // act
        (var success, var message) = await _FeatureFlagService.SaveFeatureFlagAsync(updateFeatureFlag);

        // assert
        Assert.False(success);
        Assert.Equal(Core.ErrorInvalidId, message);
    }

    [Fact]
    public async Task SaveFeatureFlagAsync_CreateWithDuplicateName_ReturnsDuplicateError() {
        // arrange
        var duplicateFeatureFlag = new FeatureFlagModel {
            Name = _Fixture.TestFeatureFlag.Name, IsEnabled = true
        };

        // act
        (var success, var message) = await _FeatureFlagService.SaveFeatureFlagAsync(duplicateFeatureFlag);

        // assert
        Assert.False(success);
        Assert.Equal(Flags.ErrorDuplicateName, message);
    }

    [Fact]
    public async Task SaveFeatureFlagAsync_UpdateWithDuplicateName_ReturnsDuplicateError() {
        // arrange
        var originalName = "duplicate";
        var duplicateFeatureFlag = new FeatureFlagModel {
            Name = originalName, IsEnabled = true
        };
        await _FeatureFlagService.SaveFeatureFlagAsync(duplicateFeatureFlag);

        var featureFlagCopy = (await _FeatureFlagService.GetAllFeatureFlagsAsync()).FirstOrDefault(x => x.Name == originalName);
        var updateFeatureFlag = featureFlagCopy! with { Name = _Fixture.TestFeatureFlag.Name };

        // act
        (var success, var message) = await _FeatureFlagService.SaveFeatureFlagAsync(updateFeatureFlag);

        // assert
        Assert.False(success);
        Assert.Equal(Flags.ErrorDuplicateName, message);
    }

    [Fact]
    public async Task SaveFeatureFlagAsync_WithConcurrentChanges_ReturnsConcurrencyError() {
        // arrange
        var originalName = "concurrency1";
        var createFeatureFlag = new FeatureFlagModel {
            Name = originalName, IsEnabled = true, UpdatedDate = DateTime.UtcNow
        };
        await _FeatureFlagService.SaveFeatureFlagAsync(createFeatureFlag);

        var featureFlagCopy = (await _FeatureFlagService.GetAllFeatureFlagsAsync()).FirstOrDefault(x => x.Name == originalName);
        // decrement UpdatedDate to emulate an older row version
        var updateFeatureFlag = featureFlagCopy! with { Name = "concurrency1_updated", UpdatedDate = featureFlagCopy.UpdatedDate.AddSeconds(-10) };

        // act
        (var success, var message) = await _FeatureFlagService.SaveFeatureFlagAsync(updateFeatureFlag);

        await DeleteFeatureFlagIfExistsAsync(featureFlagCopy.Id);

        // assert
        Assert.False(success);
        Assert.Equal(Core.ErrorConcurrency, message);
    }

    [Fact]
    public async Task SaveFeatureFlagAsync_WithSameUpdatedDateAndConcurrentChanges_ReturnsConcurrencyError() {
        // arrange
        var originalName = "concurrency2";
        var createFeatureFlag = new FeatureFlagModel {
            Name = originalName, IsEnabled = true, UpdatedDate = DateTime.UtcNow
        };
        await _FeatureFlagService.SaveFeatureFlagAsync(createFeatureFlag);

        var featureFlagCopy = (await _FeatureFlagService.GetAllFeatureFlagsAsync()).First(x => x.Name == originalName);
        var updateFeatureFlag = featureFlagCopy with { Name = "concurrency2_updated" };
        var finalFeatureFlag = featureFlagCopy with { Name = "concurrency2_final" };
        await Task.Delay(1100);

        // act
        (var success, var message) = await GetNewFeatureFlagService().SaveFeatureFlagAsync(updateFeatureFlag);
        (var success2, var message2) = await GetNewFeatureFlagService().SaveFeatureFlagAsync(finalFeatureFlag);

        await DeleteFeatureFlagIfExistsAsync(featureFlagCopy.Id);

        // assert
        Assert.True(success);
        Assert.False(success2);
        Assert.Equal(Flags.SuccessSavingFlag, message);
        Assert.Equal(Core.ErrorConcurrency, message2);
    }

    [Fact]
    public async Task SaveFeatureFlagAsync_WithIncrementedUpdatedDateAndConcurrentChanges_ReturnsSuccess() {
        // arrange
        var originalName = "concurrency3";
        var createFeatureFlag = new FeatureFlagModel {
            Name = originalName, IsEnabled = true, UpdatedDate = DateTime.UtcNow
        };
        await _FeatureFlagService.SaveFeatureFlagAsync(createFeatureFlag);

        var featureFlagCopy = (await _FeatureFlagService.GetAllFeatureFlagsAsync()).FirstOrDefault(x => x.Name == originalName);
        var updateFeatureFlag = featureFlagCopy! with { Name = "concurrency3_updated" };
        var finalFeatureFlag = featureFlagCopy with { Name = "concurrency3_final", UpdatedDate = featureFlagCopy.UpdatedDate.AddSeconds(10) };

        // act
        (var success, var message) = await _FeatureFlagService.SaveFeatureFlagAsync(updateFeatureFlag);
        (var success2, var message2) = await _FeatureFlagService.SaveFeatureFlagAsync(finalFeatureFlag);

        await DeleteFeatureFlagIfExistsAsync(featureFlagCopy.Id);

        // assert
        Assert.True(success);
        Assert.True(success2);
        Assert.Equal(Flags.SuccessSavingFlag, message);
        Assert.Equal(Flags.SuccessSavingFlag, message2);
    }

    [Fact]
    public async Task DeleteFeatureFlagAsync_WithValidFeatureFlag_DeletesFeatureFlag() {
        // arrange
        var featureFlagToDelete = new FeatureFlag {
            Id = -100, Name = "delete", IsEnabled = true
        };
        using (var dbContext = _Fixture.CreateContext()) {
            dbContext.FeatureFlags.Add(featureFlagToDelete);
            await dbContext.SaveChangesAsync();
        }

        // act
        var result = await _FeatureFlagService.DeleteFeatureFlagAsync(featureFlagToDelete.Id);
        var deletedFeatureFlag = _Fixture.CreateContext().FeatureFlags.FirstOrDefault(x => x.Id == featureFlagToDelete.Id);

        // assert
        Assert.True(result);
        Assert.Null(deletedFeatureFlag);
    }

    [Fact]
    public async Task DeleteFeatureFlagAsync_WithInvalidFeatureFlagId_ReturnsFalse() {
        // arrange
        var featureFlagIdToDelete = -200;

        // act
        var result = await _FeatureFlagService.DeleteFeatureFlagAsync(featureFlagIdToDelete);

        // assert
        Assert.False(result);
    }

    [Fact]
    public void ClearCache_ClearsMemoryCache() {
        // act
        _FeatureFlagService.ClearCache();

        // assert
        _MockMemoryCache.Verify(x => x.Remove(It.IsAny<object>()), Times.Once);
    }
}
