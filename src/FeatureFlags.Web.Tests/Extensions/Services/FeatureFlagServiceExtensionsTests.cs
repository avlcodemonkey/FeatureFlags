using FeatureFlags.Constants;
using FeatureFlags.Domain.Models;
using FeatureFlags.Extensions.Services;
using FeatureFlags.Models;
using Microsoft.FeatureManagement.FeatureFilters;

namespace FeatureFlags.Web.Tests.Extensions.Services;

public class FeatureFlagServiceExtensionsTests {
    [Fact]
    public void SelectSingleAsModel_ReturnsProjectedModel() {
        // arrange
        var featureFlag = new FeatureFlag {
            Id = 1,
            Name = "test",
            Status = true,
            CreatedDate = DateTime.MinValue,
            UpdatedDate = DateTime.MinValue,
            Filters = new List<FeatureFlagFilter>
            {
                new() {
                    Id = 10,
                    FilterType = (int)FilterTypes.Targeting,
                    Users = new List<FeatureFlagFilterUser>
                    {
                        new() { User = "user1", Include = true },
                        new() { User = "user2", Include = false }
                    },
                    TimeStart = DateTime.UtcNow,
                    TimeEnd = DateTime.UtcNow.AddHours(1),
                    TimeRecurrenceType = (int)RecurrencePatternType.Weekly,
                    TimeRecurrenceInterval = 2,
                    TimeRecurrenceDaysOfWeek = "Monday,Wednesday",
                    TimeRecurrenceFirstDayOfWeek = "Monday",
                    TimeRecurrenceRangeType = (int)RecurrenceRangeType.EndDate,
                    TimeRecurrenceEndDate = DateTime.UtcNow.AddDays(7),
                    TimeRecurrenceNumberOfOccurrences = 5,
                    PercentageValue = 75,
                    JSON = "{\"key\":\"value\"}",
                    UpdatedDate = DateTime.UtcNow
                }
            }
        };
        var featureFlags = new List<FeatureFlag> { featureFlag }.AsQueryable();

        // act
        var models = featureFlags.SelectAsModel().ToList();

        // assert
        Assert.NotNull(models);
        var singleModel = Assert.Single(models);

        Assert.Equal(featureFlag.Id, singleModel.Id);
        Assert.Equal(featureFlag.Name, singleModel.Name);
        Assert.Equal(featureFlag.Status, singleModel.Status);
        Assert.Equal(featureFlag.UpdatedDate, singleModel.UpdatedDate);

        // Filters mapping assertions
        var filterModel = Assert.Single(singleModel.Filters!);
        Assert.Equal(10, filterModel.Id);
        Assert.Equal(FilterTypes.Targeting, filterModel.FilterType);
        Assert.Equal(new[] { "user1" }, filterModel.TargetUsers);
        Assert.Equal(new[] { "user2" }, filterModel.ExcludeUsers);
        Assert.Equal(featureFlag.Filters[0].TimeStart, filterModel.TimeStart);
        Assert.Equal(featureFlag.Filters[0].TimeEnd, filterModel.TimeEnd);
        Assert.Equal(RecurrencePatternType.Weekly, filterModel.TimeRecurrenceType);
        Assert.Equal(2, filterModel.TimeRecurrenceInterval);
        Assert.Equal(new[] { "Monday", "Wednesday" }, filterModel.TimeRecurrenceDaysOfWeek);
        Assert.Equal("Monday", filterModel.TimeRecurrenceFirstDayOfWeek);
        Assert.Equal(RecurrenceRangeType.EndDate, filterModel.TimeRecurrenceRangeType);
        Assert.Equal(featureFlag.Filters[0].TimeRecurrenceEndDate, filterModel.TimeRecurrenceEndDate);
        Assert.Equal(5, filterModel.TimeRecurrenceNumberOfOccurrences);
        Assert.Equal(75, filterModel.PercentageValue);
        Assert.Equal("{\"key\":\"value\"}", filterModel.JSON);
        Assert.Equal(featureFlag.Filters[0].UpdatedDate, filterModel.UpdatedDate);
    }

    [Fact]
    public void SelectMultipleAsModel_ReturnsProjectedModels() {
        // arrange
        var featureFlag1 = new FeatureFlag {
            Id = 1,
            Name = "test",
            Status = true,
            CreatedDate = DateTime.MinValue,
            UpdatedDate = DateTime.MinValue,
            Filters = new List<FeatureFlagFilter>
            {
                new() {
                    Id = 10,
                    FilterType = (int)FilterTypes.Targeting,
                    Users = new List<FeatureFlagFilterUser>
                    {
                        new() { User = "user1", Include = true }
                    },
                    UpdatedDate = DateTime.UtcNow
                }
            }
        };
        var featureFlag2 = new FeatureFlag {
            Id = 2,
            Name = "flag 2",
            Status = false,
            CreatedDate = DateTime.MaxValue,
            UpdatedDate = DateTime.MaxValue,
            Filters = new List<FeatureFlagFilter>
            {
                new() {
                    Id = 20,
                    FilterType = (int)FilterTypes.Percentage,
                    PercentageValue = 50,
                    UpdatedDate = DateTime.UtcNow
                }
            }
        };
        var featureFlags = new List<FeatureFlag> { featureFlag1, featureFlag2 }.AsQueryable();

        // act
        var models = featureFlags.SelectAsModel().ToList();

        // assert
        Assert.NotNull(models);
        Assert.Equal(2, models.Count);

        // Filters mapping assertions for first flag
        var filterModel1 = Assert.Single(models[0].Filters!);
        Assert.Equal(10, filterModel1.Id);
        Assert.Equal(FilterTypes.Targeting, filterModel1.FilterType);
        Assert.Equal(new[] { "user1" }, filterModel1.TargetUsers);

        // Filters mapping assertions for second flag
        var filterModel2 = Assert.Single(models[1].Filters!);
        Assert.Equal(20, filterModel2.Id);
        Assert.Equal(FilterTypes.Percentage, filterModel2.FilterType);
        Assert.Equal(50, filterModel2.PercentageValue);
    }

    [Fact]
    public void Index_ReturnsIdAsString_WhenIdIsPositive() {
        var model = new FeatureFlagFilterModel { Id = 42, FilterType = FilterTypes.Targeting, UpdatedDate = DateTime.UtcNow };
        Assert.Equal("42", model.Index);
    }

    [Fact]
    public void Index_ReturnsPlaceholder_WhenIdIsZeroOrNegative() {
        var model = new FeatureFlagFilterModel { Id = 0, FilterType = FilterTypes.Targeting, UpdatedDate = DateTime.UtcNow };
        Assert.Equal(FilterConstants.IndexPlaceholder, model.Index);

        var modelNeg = new FeatureFlagFilterModel { Id = -1, FilterType = FilterTypes.Targeting, UpdatedDate = DateTime.UtcNow };
        Assert.Equal(FilterConstants.IndexPlaceholder, modelNeg.Index);
    }

    [Fact]
    public void TargetingProperties_CanBeSetAndRetrieved() {
        var model = new FeatureFlagFilterModel {
            Id = 1,
            FilterType = FilterTypes.Targeting,
            TargetUsers = new[] { "user1", "user2" },
            ExcludeUsers = new[] { "user3" },
            UpdatedDate = DateTime.UtcNow
        };

        Assert.Equal(new[] { "user1", "user2" }, model.TargetUsers);
        Assert.Equal(new[] { "user3" }, model.ExcludeUsers);
    }

    [Fact]
    public void TimeWindowProperties_CanBeSetAndRetrieved() {
        var now = DateTime.UtcNow;
        var model = new FeatureFlagFilterModel {
            Id = 2,
            FilterType = FilterTypes.TimeWindow,
            TimeStart = now,
            TimeEnd = now.AddHours(1),
            TimeRecurrenceType = RecurrencePatternType.Weekly,
            TimeRecurrenceInterval = 2,
            TimeRecurrenceDaysOfWeek = new[] { "Monday", "Wednesday" },
            TimeRecurrenceFirstDayOfWeek = "Monday",
            TimeRecurrenceRangeType = RecurrenceRangeType.EndDate,
            TimeRecurrenceEndDate = now.AddDays(7),
            TimeRecurrenceNumberOfOccurrences = 5,
            UpdatedDate = now
        };

        Assert.Equal(now, model.TimeStart);
        Assert.Equal(now.AddHours(1), model.TimeEnd);
        Assert.Equal(RecurrencePatternType.Weekly, model.TimeRecurrenceType);
        Assert.Equal(2, model.TimeRecurrenceInterval);
        Assert.Equal(new[] { "Monday", "Wednesday" }, model.TimeRecurrenceDaysOfWeek);
        Assert.Equal("Monday", model.TimeRecurrenceFirstDayOfWeek);
        Assert.Equal(RecurrenceRangeType.EndDate, model.TimeRecurrenceRangeType);
        Assert.Equal(now.AddDays(7), model.TimeRecurrenceEndDate);
        Assert.Equal(5, model.TimeRecurrenceNumberOfOccurrences);
    }

    [Fact]
    public void PercentageValue_CanBeSetAndRetrieved() {
        var model = new FeatureFlagFilterModel {
            Id = 3,
            FilterType = FilterTypes.Percentage,
            PercentageValue = 75,
            UpdatedDate = DateTime.UtcNow
        };

        Assert.Equal(75, model.PercentageValue);
    }

    [Fact]
    public void JSON_CanBeSetAndRetrieved() {
        var json = "{\"key\":\"value\"}";
        var model = new FeatureFlagFilterModel {
            Id = 4,
            FilterType = FilterTypes.JSON,
            JSON = json,
            UpdatedDate = DateTime.UtcNow
        };

        Assert.Equal(json, model.JSON);
    }

    [Fact]
    public void Properties_DefaultToNullOrEmpty() {
        var model = new FeatureFlagFilterModel {
            Id = 5,
            FilterType = FilterTypes.Targeting,
            UpdatedDate = DateTime.UtcNow
        };

        Assert.Null(model.TargetUsers);
        Assert.Null(model.ExcludeUsers);
        Assert.Null(model.TimeStart);
        Assert.Null(model.TimeEnd);
        Assert.Null(model.TimeRecurrenceType);
        Assert.Null(model.TimeRecurrenceInterval);
        Assert.Null(model.TimeRecurrenceDaysOfWeek);
        Assert.Null(model.TimeRecurrenceFirstDayOfWeek);
        Assert.Null(model.TimeRecurrenceRangeType);
        Assert.Null(model.TimeRecurrenceEndDate);
        Assert.Null(model.TimeRecurrenceNumberOfOccurrences);
        Assert.Null(model.PercentageValue);
        Assert.Null(model.JSON);
    }
}
