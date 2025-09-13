using FeatureFlags.Domain.Models;
using FeatureFlags.Models;
using FeatureFlags.Resources;
using FeatureFlags.Services;
using FeatureFlags.Web.Tests.Fixtures;
using Microsoft.FeatureManagement.FeatureFilters;

namespace FeatureFlags.Web.Tests.Services;

[Collection(nameof(DatabaseCollection))]
public class FeatureFlagServiceTests {
    private readonly DatabaseFixture _Fixture;
    private readonly FeatureFlagService _FeatureFlagService;

    private FeatureFlagService GetNewFeatureFlagService() => new(_Fixture.CreateContext());

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
        Assert.Contains(featureFlags, x => x.RequirementType == Constants.RequirementType.All);
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
        Assert.Equal(testFeatureFlag.Status, featureFlag.Status);
        Assert.Equal(Constants.RequirementType.All, featureFlag.RequirementType);
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
    public async Task GetFeatureFlagByNameAsync_ReturnsTestFeatureFlagModel() {
        // arrange
        var testFeatureFlag = _Fixture.TestFeatureFlag;

        // act
        var featureFlag = await _FeatureFlagService.GetFeatureFlagByNameAsync(testFeatureFlag.Name);

        // assert
        Assert.NotNull(featureFlag);
        Assert.IsType<FeatureFlagModel>(featureFlag);
        Assert.Equal(testFeatureFlag.Name, featureFlag.Name);
        Assert.Equal(testFeatureFlag.Status, featureFlag.Status);
        Assert.Equal(Constants.RequirementType.All, featureFlag.RequirementType);
    }

    [Fact]
    public async Task GetFeatureFlagByNameAsync_ReturnsCaseInsensitiveTestFeatureFlagModel() {
        // arrange
        var testFeatureFlag = _Fixture.TestFeatureFlag;

        // act
        var featureFlag = await _FeatureFlagService.GetFeatureFlagByNameAsync(testFeatureFlag.Name.ToUpper());

        // assert
        Assert.NotNull(featureFlag);
        Assert.IsType<FeatureFlagModel>(featureFlag);
        Assert.Equal(testFeatureFlag.Name, featureFlag.Name);
        Assert.Equal(testFeatureFlag.Status, featureFlag.Status);
        Assert.Equal(Constants.RequirementType.All, featureFlag.RequirementType);
    }

    [Fact]
    public async Task SaveFeatureFlagAsync_CreateFeatureFlag_SavesNewFeatureFlag() {
        // arrange
        var createFeatureFlag = new FeatureFlagModel {
            Name = "create", Status = true, RequirementType = Constants.RequirementType.All
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
        Assert.Equal(createFeatureFlag.Status, newFeatureFlag.Status);
        Assert.Equal(createFeatureFlag.RequirementType, newFeatureFlag.RequirementType);
    }

    [Fact]
    public async Task SaveFeatureFlagAsync_UpdateFeatureFlag_SavesChanges() {
        // arrange
        var originalName = "original";
        var createFeatureFlag = new FeatureFlagModel {
            Name = originalName, Status = true, UpdatedDate = DateTime.UtcNow, RequirementType = Constants.RequirementType.All
        };
        await _FeatureFlagService.SaveFeatureFlagAsync(createFeatureFlag);

        var featureFlagId = (await _FeatureFlagService.GetAllFeatureFlagsAsync()).FirstOrDefault(x => x.Name == originalName)!.Id;
        var newName = "newName";
        var updateFeatureFlag = new FeatureFlagModel {
            Id = featureFlagId, Name = newName, Status = false, UpdatedDate = DateTime.UtcNow, RequirementType = Constants.RequirementType.Any
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
        Assert.False(updatedFeatureFlag.Status);
    }

    [Fact]
    public async Task SaveFeatureFlagAsync_UpdateInvalidFeatureFlagId_ReturnsFalse() {
        // arrange
        var updateFeatureFlag = new FeatureFlagModel {
            Id = 999, Name = "update", Status = true
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
            Name = _Fixture.TestFeatureFlag.Name, Status = true, RequirementType = Constants.RequirementType.All
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
            Name = originalName, Status = true, RequirementType = Constants.RequirementType.All
        };
        await _FeatureFlagService.SaveFeatureFlagAsync(duplicateFeatureFlag);

        var featureFlagCopy = (await _FeatureFlagService.GetAllFeatureFlagsAsync()).FirstOrDefault(x => x.Name == originalName);
        var updateFeatureFlag = featureFlagCopy! with { Name = _Fixture.TestFeatureFlag.Name, RequirementType = Constants.RequirementType.All };

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
            Name = originalName, Status = true, UpdatedDate = DateTime.UtcNow
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
            Name = originalName, Status = true, UpdatedDate = DateTime.UtcNow
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
            Name = originalName, Status = true, UpdatedDate = DateTime.UtcNow
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
            Id = -100, Name = "delete", Status = true
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
    public async Task MapToEntity_MapsBasicProperties() {
        var model = new FeatureFlagModel {
            Name = "TestFlag",
            Status = true,
            RequirementType = Constants.RequirementType.All
        };
        var entity = new FeatureFlag();

        await InvokeMapToEntity(model, entity);

        Assert.Equal(model.Name, entity.Name);
        Assert.Equal(model.Status, entity.Status);
        Assert.Equal((int)model.RequirementType, entity.RequirementType);
    }

    [Fact]
    public async Task MapToEntity_MapsTargetingFilter() {
        var filterModel = new FeatureFlagFilterModel {
            FilterType = Constants.FilterTypes.Targeting,
            TargetUsers = new[] { "user1", "user2" },
            ExcludeUsers = new[] { "user3" }
        };
        var model = new FeatureFlagModel {
            Name = "TargetingFlag",
            Status = true,
            RequirementType = Constants.RequirementType.All,
            Filters = new[] { filterModel }
        };
        var entity = new FeatureFlag();

        await InvokeMapToEntity(model, entity);

        var filter = entity.Filters.Single();
        Assert.Equal((int)Constants.FilterTypes.Targeting, filter.FilterType);
        Assert.Equal(3, filter.Users.Count);
        Assert.Contains(filter.Users, u => u.User == "user1" && u.Include);
        Assert.Contains(filter.Users, u => u.User == "user2" && u.Include);
        Assert.Contains(filter.Users, u => u.User == "user3" && !u.Include);
    }

    [Fact]
    public async Task MapToEntity_MapsTimeWindowFilter() {
        var now = DateTime.UtcNow;
        var filterModel = new FeatureFlagFilterModel {
            FilterType = Constants.FilterTypes.TimeWindow,
            TimeStart = now,
            TimeEnd = now.AddHours(1),
            TimeRecurrenceType = RecurrencePatternType.Weekly,
            TimeRecurrenceInterval = 2,
            TimeRecurrenceDaysOfWeek = new[] { "Monday", "Wednesday" },
            TimeRecurrenceFirstDayOfWeek = "Monday",
            TimeRecurrenceRangeType = RecurrenceRangeType.EndDate,
            TimeRecurrenceEndDate = now.AddDays(7)
        };
        var model = new FeatureFlagModel {
            Name = "TimeWindowFlag",
            Status = true,
            RequirementType = Constants.RequirementType.All,
            Filters = new[] { filterModel }
        };
        var entity = new FeatureFlag();

        await InvokeMapToEntity(model, entity);

        var filter = entity.Filters.Single();
        Assert.Equal((int)Constants.FilterTypes.TimeWindow, filter.FilterType);
        Assert.Equal(now, filter.TimeStart);
        Assert.Equal(now.AddHours(1), filter.TimeEnd);
        Assert.Equal((int)RecurrencePatternType.Weekly, filter.TimeRecurrenceType);
        Assert.Equal(2, filter.TimeRecurrenceInterval);
        Assert.Equal("Monday,Wednesday", filter.TimeRecurrenceDaysOfWeek);
        Assert.Equal("Monday", filter.TimeRecurrenceFirstDayOfWeek);
        Assert.Equal((int)RecurrenceRangeType.EndDate, filter.TimeRecurrenceRangeType);
        Assert.Equal(now.AddDays(7), filter.TimeRecurrenceEndDate);
    }

    [Fact]
    public async Task MapToEntity_MapsPercentageFilter() {
        var filterModel = new FeatureFlagFilterModel {
            FilterType = Constants.FilterTypes.Percentage,
            PercentageValue = 42
        };
        var model = new FeatureFlagModel {
            Name = "PercentageFlag",
            Status = true,
            RequirementType = Constants.RequirementType.All,
            Filters = new[] { filterModel }
        };
        var entity = new FeatureFlag();

        await InvokeMapToEntity(model, entity);

        var filter = entity.Filters.Single();
        Assert.Equal((int)Constants.FilterTypes.Percentage, filter.FilterType);
        Assert.Equal(42, filter.PercentageValue);
    }

    [Fact]
    public async Task MapToEntity_MapsJSONFilter() {
        var filterModel = new FeatureFlagFilterModel {
            FilterType = Constants.FilterTypes.JSON,
            JSON = "{\"key\":\"value\"}"
        };
        var model = new FeatureFlagModel {
            Name = "JsonFlag",
            Status = true,
            RequirementType = Constants.RequirementType.All,
            Filters = new[] { filterModel }
        };
        var entity = new FeatureFlag();

        await InvokeMapToEntity(model, entity);

        var filter = entity.Filters.Single();
        Assert.Equal((int)Constants.FilterTypes.JSON, filter.FilterType);
        Assert.Equal("{\"key\":\"value\"}", filter.JSON);
    }

    [Fact]
    public async Task MapToEntity_ClearsPropertiesForNonMatchingFilterTypes() {
        var filterModel = new FeatureFlagFilterModel {
            FilterType = Constants.FilterTypes.Percentage,
            TimeStart = DateTime.UtcNow,
            TimeEnd = DateTime.UtcNow.AddHours(1),
            JSON = "{\"shouldBeNull\":true}",
            PercentageValue = 42
        };
        var model = new FeatureFlagModel {
            Name = "ClearFlag",
            Status = true,
            RequirementType = Constants.RequirementType.All,
            Filters = new[] { filterModel }
        };
        var entity = new FeatureFlag();

        await InvokeMapToEntity(model, entity);

        var filter = entity.Filters.Single();
        Assert.Null(filter.TimeStart);
        Assert.Null(filter.TimeEnd);
        Assert.Null(filter.JSON);
        Assert.Equal(42, filter.PercentageValue); // If PercentageValue is set, otherwise null
    }

    [Fact]
    public async Task MapToEntity_MapsMultipleFiltersOfDifferentTypes() {
        var now = DateTime.UtcNow;
        var model = new FeatureFlagModel {
            Name = "MultiFilterFlag",
            Status = true,
            RequirementType = Constants.RequirementType.All,
            Filters = new[] {
                new FeatureFlagFilterModel {
                    FilterType = Constants.FilterTypes.Targeting,
                    TargetUsers = new[] { "userA" }
                },
                new FeatureFlagFilterModel {
                    FilterType = Constants.FilterTypes.TimeWindow,
                    TimeStart = now,
                    TimeEnd = now.AddMinutes(30)
                },
                new FeatureFlagFilterModel {
                    FilterType = Constants.FilterTypes.Percentage,
                    PercentageValue = 99
                },
                new FeatureFlagFilterModel {
                    FilterType = Constants.FilterTypes.JSON,
                    JSON = "{\"enabled\":true}"
                }
            }
        };
        var entity = new FeatureFlag();
        await InvokeMapToEntity(model, entity);

        Assert.Equal(4, entity.Filters.Count);
        Assert.Contains(entity.Filters, f => f.FilterType == (int)Constants.FilterTypes.Targeting && f.Users.Any(u => u.User == "userA"));
        Assert.Contains(entity.Filters, f => f.FilterType == (int)Constants.FilterTypes.TimeWindow && f.TimeStart == now);
        Assert.Contains(entity.Filters, f => f.FilterType == (int)Constants.FilterTypes.Percentage && f.PercentageValue == 99);
        Assert.Contains(entity.Filters, f => f.FilterType == (int)Constants.FilterTypes.JSON && f.JSON == "{\"enabled\":true}");
    }

    [Fact]
    public async Task MapToEntity_HandlesNullAndEmptyFilters() {
        var modelWithNull = new FeatureFlagModel {
            Name = "NullFiltersFlag",
            Status = true,
            RequirementType = Constants.RequirementType.All,
            Filters = null
        };
        var entityNull = new FeatureFlag();
        await InvokeMapToEntity(modelWithNull, entityNull);
        Assert.Empty(entityNull.Filters);

        var modelWithEmpty = new FeatureFlagModel {
            Name = "EmptyFiltersFlag",
            Status = true,
            RequirementType = Constants.RequirementType.All,
            Filters = Array.Empty<FeatureFlagFilterModel>()
        };
        var entityEmpty = new FeatureFlag();
        await InvokeMapToEntity(modelWithEmpty, entityEmpty);
        Assert.Empty(entityEmpty.Filters);
    }

    [Fact]
    public async Task MapToEntity_ReusesExistingFilterById() {
        var existingFilter = new FeatureFlagFilter {
            Id = 123,
            FilterType = (int)Constants.FilterTypes.Targeting,
            Users = new List<FeatureFlagFilterUser> { new() { User = "existingUser", Include = false } }
        };
        var existingFlag = new FeatureFlag {
            Name = "ReuseFilterFlag",
            Status = true,
            RequirementType = (int)Constants.RequirementType.All,
            Filters = [existingFilter]
        };
        var dbContext = _Fixture.CreateContext();
        dbContext.FeatureFlags.Add(existingFlag);
        dbContext.SaveChanges();

        var filterModel = new FeatureFlagFilterModel {
            Id = 123,
            FilterType = Constants.FilterTypes.Targeting,
            TargetUsers = new[] { "existingUser" }
        };
        var model = new FeatureFlagModel {
            Name = "ReuseFilterFlag",
            Status = true,
            RequirementType = Constants.RequirementType.All,
            Filters = new[] { filterModel }
        };
        var entity = new FeatureFlag { Id = existingFilter.FeatureFlagId };

        await InvokeMapToEntity(model, entity);

        var filter = entity.Filters.Single();
        Assert.Equal(123, filter.Id);
        Assert.Contains(filter.Users, u => u.User == "existingUser" && u.Include);
    }

    [Fact]
    public async Task MapToEntity_ClearsUsersForNonTargetingFilter() {
        var filterModel = new FeatureFlagFilterModel {
            FilterType = Constants.FilterTypes.Percentage,
            TargetUsers = new[] { "shouldNotAppear" }
        };
        var model = new FeatureFlagModel {
            Name = "ClearUsersFlag",
            Status = true,
            RequirementType = Constants.RequirementType.All,
            Filters = new[] { filterModel }
        };
        var entity = new FeatureFlag();

        await InvokeMapToEntity(model, entity);

        var filter = entity.Filters.Single();
        Assert.Empty(filter.Users);
    }

    [Fact]
    public async Task MapToEntity_ClearsRecurrencePropertiesWhenNotSet() {
        var filterModel = new FeatureFlagFilterModel {
            FilterType = Constants.FilterTypes.TimeWindow,
            TimeStart = DateTime.UtcNow,
            TimeEnd = DateTime.UtcNow.AddHours(1),
            TimeRecurrenceType = null
        };
        var model = new FeatureFlagModel {
            Name = "ClearRecurrenceFlag",
            Status = true,
            RequirementType = Constants.RequirementType.All,
            Filters = new[] { filterModel }
        };
        var entity = new FeatureFlag();

        await InvokeMapToEntity(model, entity);

        var filter = entity.Filters.Single();
        Assert.Null(filter.TimeRecurrenceInterval);
        Assert.Null(filter.TimeRecurrenceDaysOfWeek);
        Assert.Null(filter.TimeRecurrenceFirstDayOfWeek);
        Assert.Null(filter.TimeRecurrenceRangeType);
        Assert.Null(filter.TimeRecurrenceEndDate);
        Assert.Null(filter.TimeRecurrenceNumberOfOccurrences);
    }

    [Fact]
    public async Task MapToEntity_HandlesNullAndEmptyUserLists() {
        var filterModel = new FeatureFlagFilterModel {
            FilterType = Constants.FilterTypes.Targeting,
            TargetUsers = null,
            ExcludeUsers = Array.Empty<string>()
        };
        var model = new FeatureFlagModel {
            Name = "NullUserListsFlag",
            Status = true,
            RequirementType = Constants.RequirementType.All,
            Filters = new[] { filterModel }
        };
        var entity = new FeatureFlag();

        await InvokeMapToEntity(model, entity);

        var filter = entity.Filters.Single();
        Assert.Empty(filter.Users);
    }

    [Fact]
    public async Task MapToEntity_ClearsPercentageAndJSONForNonMatchingTypes() {
        var filterModel = new FeatureFlagFilterModel {
            FilterType = Constants.FilterTypes.TimeWindow,
            PercentageValue = 77,
            JSON = "{\"shouldBeNull\":true}"
        };
        var model = new FeatureFlagModel {
            Name = "ClearOtherPropsFlag",
            Status = true,
            RequirementType = Constants.RequirementType.All,
            Filters = new[] { filterModel }
        };
        var entity = new FeatureFlag();

        await InvokeMapToEntity(model, entity);

        var filter = entity.Filters.Single();
        Assert.Null(filter.PercentageValue);
        Assert.Null(filter.JSON);
    }

    private async Task InvokeMapToEntity(FeatureFlagModel model, FeatureFlag entity) {
        // Use reflection to invoke private method
        var method = typeof(FeatureFlagService).GetMethod("MapToEntity", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        await (Task)method.Invoke(_FeatureFlagService, new object[] { model, entity, default(CancellationToken) });
    }
}
