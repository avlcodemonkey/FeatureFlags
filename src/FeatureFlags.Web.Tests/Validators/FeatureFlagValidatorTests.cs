using FeatureFlags.Constants;
using FeatureFlags.Models;
using FeatureFlags.Validators;
using Microsoft.FeatureManagement.FeatureFilters;

namespace FeatureFlags.Web.Tests.Validators;

public class FeatureFlagValidatorTests {
    [Fact]
    public void Validate_ReturnsEmpty_WhenNoFilters() {
        var model = new FeatureFlagModel {
            Filters = null
        };

        var results = FeatureFlagValidator.Validate(model);

        Assert.Empty(results);
    }

    [Fact]
    public void ValidateTargetingFilter_ReturnsError_WhenNoUsers() {
        var model = new FeatureFlagModel {
            Filters = new[]
            {
                new FeatureFlagFilterModel
                {
                    Index = "0",
                    FilterType = FilterTypes.Targeting,
                    TargetUsers = new List<string>(),
                    ExcludeUsers = new List<string>()
                }
            }
        };

        var results = FeatureFlagValidator.Validate(model);

        Assert.Contains(results, r => r.ErrorMessage == Resources.Flags.ErrorTargetingNoUsers);
    }

    [Fact]
    public void ValidateTimeWindowFilter_ReturnsError_WhenNoStartOrEnd() {
        var model = new FeatureFlagModel {
            Filters = new[]
            {
                new FeatureFlagFilterModel
                {
                    Index = "0",
                    FilterType = FilterTypes.TimeWindow,
                }
            }
        };

        var results = FeatureFlagValidator.Validate(model);

        Assert.Contains(results, r => r.ErrorMessage == Resources.Flags.ErrorTimeStartOrEndRequired);
    }

    [Fact]
    public void ValidateTimeWindowFilter_ReturnsError_WhenEndBeforeStart() {
        var model = new FeatureFlagModel {
            Filters = new[]
            {
                new FeatureFlagFilterModel
                {
                    Index = "0",
                    FilterType = FilterTypes.TimeWindow,
                    TimeStart = new DateTime(2025, 1, 2, 0, 0, 0, DateTimeKind.Utc),
                    TimeEnd = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            }
        };

        var results = FeatureFlagValidator.Validate(model);

        Assert.Contains(results, r => r.ErrorMessage == Resources.Flags.ErrorTimeWindowEndBeforeStart);
    }

    [Fact]
    public void ValidateTimeWindowFilter_ReturnsError_WhenRecurrenceIntervalInvalid() {
        var model = new FeatureFlagModel {
            Filters = new[]
            {
                new FeatureFlagFilterModel
                {
                    Index = "0",
                    FilterType = FilterTypes.TimeWindow,
                    TimeRecurrenceType = RecurrencePatternType.Weekly,
                    TimeRecurrenceInterval = 0
                }
            }
        };

        var results = FeatureFlagValidator.Validate(model);

        Assert.Contains(results, r => r.ErrorMessage == Resources.Flags.ErrorRecurrenceIntervalRequired);
    }

    [Fact]
    public void ValidateTimeWindowFilter_ReturnsError_WhenWeeklyDaysMissing() {
        var model = new FeatureFlagModel {
            Filters = new[]
            {
                new FeatureFlagFilterModel
                {
                    Index = "0",
                    FilterType = FilterTypes.TimeWindow,
                    TimeRecurrenceType = RecurrencePatternType.Weekly,
                    TimeRecurrenceInterval = 1,
                    TimeRecurrenceDaysOfWeek = new List<string>()
                }
            }
        };

        var results = FeatureFlagValidator.Validate(model);

        Assert.Contains(results, r => r.ErrorMessage == Resources.Flags.ErrorRecurrenceDaysOfWeekRequired);
    }

    [Fact]
    public void ValidateTimeWindowFilter_ReturnsError_WhenWeeklyFirstDayMissing() {
        var model = new FeatureFlagModel {
            Filters = new[]
            {
                new FeatureFlagFilterModel
                {
                    Index = "0",
                    FilterType = FilterTypes.TimeWindow,
                    TimeRecurrenceType = RecurrencePatternType.Weekly,
                    TimeRecurrenceInterval = 1,
                    TimeRecurrenceDaysOfWeek = new List<string> { "Monday" },
                    TimeRecurrenceFirstDayOfWeek = ""
                }
            }
        };

        var results = FeatureFlagValidator.Validate(model);

        Assert.Contains(results, r => r.ErrorMessage == Resources.Flags.ErrorRecurrenceFirstDayOfWeekRequired);
    }

    [Fact]
    public void ValidateTimeWindowFilter_ReturnsError_WhenEndDateRequired() {
        var model = new FeatureFlagModel {
            Filters = new[]
            {
                new FeatureFlagFilterModel
                {
                    Index = "0",
                    FilterType = FilterTypes.TimeWindow,
                    TimeRecurrenceRangeType = RecurrenceRangeType.EndDate
                    // TimeRecurrenceEndDate is missing
                }
            }
        };

        var results = FeatureFlagValidator.Validate(model);

        Assert.Contains(results, r => r.ErrorMessage == Resources.Flags.ErrorRecurrenceEndDateRequired);
    }

    [Fact]
    public void ValidateTimeWindowFilter_ReturnsError_WhenNumberOfOccurrencesRequired() {
        var model = new FeatureFlagModel {
            Filters = new[]
            {
                new FeatureFlagFilterModel
                {
                    Index = "0",
                    FilterType = FilterTypes.TimeWindow,
                    TimeRecurrenceRangeType = RecurrenceRangeType.Numbered,
                    TimeRecurrenceNumberOfOccurrences = 0
                }
            }
        };

        var results = FeatureFlagValidator.Validate(model);

        Assert.Contains(results, r => r.ErrorMessage == Resources.Flags.ErrorRecurrenceNumberOfOccurrencesRequired);
    }

    [Fact]
    public void ValidatePercentageFilter_ReturnsError_WhenOutOfRange() {
        var model = new FeatureFlagModel {
            Filters = new[]
            {
                new FeatureFlagFilterModel
                {
                    Index = "0",
                    FilterType = FilterTypes.Percentage,
                    PercentageValue = 101
                }
            }
        };

        var results = FeatureFlagValidator.Validate(model);

        Assert.Contains(results, r => r.ErrorMessage == Resources.Flags.ErrorPercentageOutOfRange);
    }

    [Fact]
    public void ValidateJsonFilter_ReturnsError_WhenJsonMissing() {
        var model = new FeatureFlagModel {
            Filters = new[]
            {
                new FeatureFlagFilterModel
                {
                    Index = "0",
                    FilterType = FilterTypes.JSON,
                    JSON = ""
                }
            }
        };

        var results = FeatureFlagValidator.Validate(model);

        Assert.Contains(results, r => r.ErrorMessage == Resources.Flags.ErrorJsonRequired);
    }

    [Fact]
    public void Validate_ReturnsEmpty_WhenAllFiltersValid() {
        // get the first monday of 2025
        var firstMonday = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        while (firstMonday.DayOfWeek != DayOfWeek.Monday) {
            firstMonday = firstMonday.AddDays(1);
        }

        var model = new FeatureFlagModel {
            Filters = new[]
            {
                new FeatureFlagFilterModel
                {
                    Index = "0",
                    FilterType = FilterTypes.Targeting,
                    TargetUsers = new List<string> { "user1" }
                },
                new FeatureFlagFilterModel
                {
                    Index = "1",
                    FilterType = FilterTypes.TimeWindow,
                    TimeStart = firstMonday,
                    TimeEnd = firstMonday.AddDays(1),
                    TimeRecurrenceType = RecurrencePatternType.Weekly,
                    TimeRecurrenceInterval = 1,
                    TimeRecurrenceDaysOfWeek = new List<string> { "Monday" },
                    TimeRecurrenceFirstDayOfWeek = "Monday",
                    TimeRecurrenceRangeType = RecurrenceRangeType.EndDate,
                    TimeRecurrenceEndDate = new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new FeatureFlagFilterModel
                {
                    Index = "2",
                    FilterType = FilterTypes.Percentage,
                    PercentageValue = 50
                },
                new FeatureFlagFilterModel
                {
                    Index = "3",
                    FilterType = FilterTypes.JSON,
                    JSON = "{ \"name\": \"Test\" }"
                }
            }
        };

        var results = FeatureFlagValidator.Validate(model);

        Assert.Empty(results);
    }
}
