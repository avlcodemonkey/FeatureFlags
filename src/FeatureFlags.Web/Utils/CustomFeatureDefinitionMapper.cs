using System.Text.Json;
using FeatureFlags.Client;
using FeatureFlags.Constants;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.FeatureFilters;

namespace FeatureFlags.Utils;

/// <summary>
/// Provides functionality to map a <see cref="Models.FeatureFlagModel"/> to a <see cref="FeatureDefinition"/>.
/// </summary>
/// <remarks>This class is responsible for converting feature flag models, which represent the configuration of
/// feature flags in the application, into <see cref="FeatureDefinition"/> objects that are compatible with the
/// Microsoft Feature Management library. Mapping includes translating filters, requirement types, and statuses.</remarks>
public static class CustomFeatureDefinitionMapper {
    /// <summary>
    /// Maps a <see cref="Models.FeatureFlagModel"/> to a <see cref="FeatureDefinition"/>.
    /// </summary>
    /// <param name="flag">Feature flag model to map. Must not be <c>null</c>.</param>
    /// <returns><see cref="FeatureDefinition"/> representing the mapped feature flag.</returns>
    public static CustomFeatureDefinition MapToCustomFeatureDefinition(Models.FeatureFlagModel flag) {
        /*
         * A feature is OFF if status is false.
         * If status is true, then the state of the feature depends on the filters.
         * If there are no filters, then the feature is ON.
         * If there are filters, and they're met, then the feature is ON.
         * If there are filters and they aren't met then the feature is OFF.
         */

        // start by building filters
        var enabledFor = new List<CustomFeatureFilterConfiguration>();
        if (flag.Status && flag.Filters != null) {
            foreach (var filter in flag.Filters) {
                var configuration = MapFilterToConfiguration(filter);
                if (configuration != null) {
                    enabledFor.Add(configuration);
                }
            }
        }

        if (flag.Status && enabledFor.Count == 0) {
            // status is true and no filters, so make the feature always on
            enabledFor.Add(new CustomFeatureFilterConfiguration { Name = "AlwaysOn" });
        }

        return new CustomFeatureDefinition {
            Name = flag.Name,
            RequirementType = flag.RequirementType == Constants.RequirementType.Any
                ? Microsoft.FeatureManagement.RequirementType.Any
                : Microsoft.FeatureManagement.RequirementType.All,
            Status = flag.Status ? FeatureStatus.Conditional : FeatureStatus.Disabled,
            EnabledFor = enabledFor
        };
    }

    private static CustomFeatureFilterConfiguration? MapFilterToConfiguration(Models.FeatureFlagFilterModel filter) {
        if (filter.FilterType == FilterTypes.JSON) {
            return MapJsonFilter(filter);
        }

        var parameterConfig = filter.FilterType switch {
            FilterTypes.Percentage => BuildPercentageConfig(filter),
            FilterTypes.TimeWindow => BuildTimeWindowConfig(filter),
            FilterTypes.Targeting => BuildTargetingConfig(filter),
            _ => throw new InvalidOperationException($"Unsupported filter type: {filter.FilterType}")
        };

        return new CustomFeatureFilterConfiguration {
            Name = filter.FilterType switch {
                FilterTypes.Targeting => "Microsoft.Targeting",
                FilterTypes.TimeWindow => "Microsoft.TimeWindow",
                FilterTypes.Percentage => "Microsoft.Percentage",
                _ => "AlwaysOn"
            },
            Parameters = parameterConfig.AsEnumerable().ToList()
        };
    }

    private static CustomFeatureFilterConfiguration? MapJsonFilter(Models.FeatureFlagFilterModel filter) {
        if (string.IsNullOrWhiteSpace(filter.JSON)) {
            return null;
        }

        var configuration = new ConfigurationBuilder()
            .AddJsonStream(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(filter.JSON)))
            .Build();
        if (configuration == null) {
            return null;
        }

        return new CustomFeatureFilterConfiguration {
            Name = configuration["name"] ?? "",
            Parameters = configuration.GetSection("parameters").AsEnumerable().ToList()
        };
    }

    private static IConfiguration BuildPercentageConfig(Models.FeatureFlagFilterModel filter) {
        var settings = new PercentageFilterSettings {
            Value = filter.PercentageValue ?? 0
        };
        var json = JsonSerializer.Serialize(settings);
        return new ConfigurationBuilder()
            .AddJsonStream(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json)))
            .Build();
    }

    private static IConfiguration BuildTimeWindowConfig(Models.FeatureFlagFilterModel filter) {
        // use anonymous object instead of TimeWindowFilterSettings because Start and End need to be formatted strings
        var settings = new {
            Start = filter.TimeStart.HasValue ? new DateTimeOffset(DateTime.SpecifyKind(filter.TimeStart.Value, DateTimeKind.Utc)).ToString("R") : null,
            End = filter.TimeEnd.HasValue ? new DateTimeOffset(DateTime.SpecifyKind(filter.TimeEnd.Value, DateTimeKind.Utc)).ToString("R") : null,
            Recurrence = BuildRecurrence(filter)
        };

        var json = JsonSerializer.Serialize(settings);
        return new ConfigurationBuilder()
            .AddJsonStream(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json)))
            .Build();
    }

    private static Recurrence? BuildRecurrence(Models.FeatureFlagFilterModel filter) {
        if (!filter.TimeRecurrenceType.HasValue && !filter.TimeRecurrenceRangeType.HasValue) {
            return null;
        }

        return new Recurrence {
            Pattern = BuildRecurrencePattern(filter),
            Range = BuildRecurrenceRange(filter)
        };
    }

    private static RecurrencePattern? BuildRecurrencePattern(Models.FeatureFlagFilterModel filter) {
        if (!filter.TimeRecurrenceType.HasValue) {
            return null;
        }

        var isWeekly = filter.TimeRecurrenceType == RecurrencePatternType.Weekly;
        var daysOfWeek = isWeekly && filter.TimeRecurrenceDaysOfWeek != null
            ? filter.TimeRecurrenceDaysOfWeek
                .Select(x => Enum.TryParse<DayOfWeek>(x, true, out var day) ? day : (DayOfWeek?)null)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .ToList()
            : null;

        var firstDayOfWeek = isWeekly && !string.IsNullOrWhiteSpace(filter.TimeRecurrenceFirstDayOfWeek)
            && Enum.TryParse<DayOfWeek>(filter.TimeRecurrenceFirstDayOfWeek, true, out var day)
            ? day
            : DayOfWeek.Sunday;

        return new RecurrencePattern {
            Type = filter.TimeRecurrenceType.Value,
            Interval = filter.TimeRecurrenceInterval ?? 1,
            DaysOfWeek = daysOfWeek,
            FirstDayOfWeek = isWeekly ? firstDayOfWeek : DayOfWeek.Sunday
        };
    }

    private static RecurrenceRange? BuildRecurrenceRange(Models.FeatureFlagFilterModel filter) {
        if (!filter.TimeRecurrenceRangeType.HasValue) {
            return null;
        }

        var endDate = filter.TimeRecurrenceRangeType == RecurrenceRangeType.EndDate && filter.TimeRecurrenceEndDate.HasValue
            ? new DateTimeOffset(DateTime.SpecifyKind(filter.TimeRecurrenceEndDate.Value, DateTimeKind.Utc))
            : (DateTimeOffset?)null;

        var range = new RecurrenceRange { Type = filter.TimeRecurrenceRangeType.Value };

        if (filter.TimeRecurrenceRangeType == RecurrenceRangeType.Numbered && filter.TimeRecurrenceNumberOfOccurrences.HasValue) {
            range.NumberOfOccurrences = filter.TimeRecurrenceNumberOfOccurrences.Value;
        }

        if (filter.TimeRecurrenceRangeType == RecurrenceRangeType.EndDate && endDate.HasValue) {
            range.EndDate = endDate.Value;
        }

        return range;
    }

    private static IConfiguration BuildTargetingConfig(Models.FeatureFlagFilterModel filter) {
        var settings = new TargetingFilterSettings {
            Audience = new Audience {
                Users = filter.TargetUsers?.Where(x => !string.IsNullOrWhiteSpace(x)).ToList() ?? [],
                Exclusion = new BasicAudience {
                    Users = filter.ExcludeUsers?.Where(x => !string.IsNullOrWhiteSpace(x)).ToList() ?? []
                }
            }
        };
        var json = JsonSerializer.Serialize(settings);
        return new ConfigurationBuilder()
            .AddJsonStream(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json)))
            .Build();
    }
}
