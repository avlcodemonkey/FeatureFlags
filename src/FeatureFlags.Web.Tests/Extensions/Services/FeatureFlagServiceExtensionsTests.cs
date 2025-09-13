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
            Id = 1, Name = "test", Status = true, CreatedDate = DateTime.MinValue, UpdatedDate = DateTime.MinValue
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
    }

    [Fact]
    public void SelectMultipleAsModel_ReturnsProjectedModels() {
        // arrange
        var featureFlag1 = new FeatureFlag {
            Id = 1, Name = "test", Status = true, CreatedDate = DateTime.MinValue, UpdatedDate = DateTime.MinValue
        };
        var featureFlag2 = new FeatureFlag {
            Id = 2, Name = "flag 2", Status = false, CreatedDate = DateTime.MaxValue, UpdatedDate = DateTime.MaxValue
        };
        var featureFlags = new List<FeatureFlag> { featureFlag1, featureFlag2 }.AsQueryable();

        // act
        var models = featureFlags.SelectAsModel().ToList();

        // assert
        Assert.NotNull(models);
        Assert.Equal(2, models.Count);

        Assert.Collection(models,
            x => Assert.Equal(featureFlag1.Id, x.Id),
            x => Assert.Equal(featureFlag2.Id, x.Id)
        );
        Assert.Collection(models,
            x => Assert.Equal(featureFlag1.Name, x.Name),
            x => Assert.Equal(featureFlag2.Name, x.Name)
        );
        Assert.Collection(models,
            x => Assert.Equal(featureFlag1.Status, x.Status),
            x => Assert.Equal(featureFlag2.Status, x.Status)
        );
        Assert.Collection(models,
            x => Assert.Equal(featureFlag1.UpdatedDate, x.UpdatedDate),
            x => Assert.Equal(featureFlag2.UpdatedDate, x.UpdatedDate)
        );
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
