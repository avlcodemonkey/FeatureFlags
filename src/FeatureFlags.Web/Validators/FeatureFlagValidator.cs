using System.ComponentModel.DataAnnotations;
using FeatureFlags.Constants;
using FeatureFlags.Models;
using FeatureFlags.Resources;
using Microsoft.FeatureManagement.FeatureFilters;

namespace FeatureFlags.Validators;

/// <summary>
/// Custom model validator for <see cref="FeatureFlagFilterModel"/>.
/// </summary>
public static class FeatureFlagValidator {
    private const int _DaysPerWeek = 7;

    /// <summary>
    /// Validates the specified <see cref="FeatureFlagModel"/> and returns a collection of validation results.
    /// </summary>
    /// <param name="model">The feature flag model to validate. Cannot be <see langword="null"/>.</param>
    /// <returns>Collection of <see cref="ValidationResult"/> objects representing validation errors. The collection will be
    /// empty if the model is valid or if no filters are defined.</returns>
    public static IEnumerable<ValidationResult> Validate(FeatureFlagModel model) {
        var results = new List<ValidationResult>();

        if (model.Filters == null || !model.Filters.Any()) {
            return results;
        }

        foreach (var filter in model.Filters) {
            results.AddRange(ValidateTargetingFilter(filter));
            results.AddRange(ValidateTimeWindowFilter(filter));
            results.AddRange(ValidatePercentageFilter(filter));
            results.AddRange(ValidateJsonFilter(filter));
        }

        return results;
    }

    private static IEnumerable<ValidationResult> ValidateTargetingFilter(FeatureFlagFilterModel filter) {
        if (filter.FilterType != FilterTypes.Targeting) {
            yield break;
        }

        var hasTarget = filter.TargetUsers?.Any(u => !string.IsNullOrWhiteSpace(u)) == true;
        var hasExclude = filter.ExcludeUsers?.Any(u => !string.IsNullOrWhiteSpace(u)) == true;
        if (!hasTarget && !hasExclude) {
            yield return new ValidationResult(Flags.ErrorTargetingNoUsers, [$"Filters[{filter.Index}].{nameof(filter.TargetUsers)}"]);
        }
    }

    private static IEnumerable<ValidationResult> ValidateTimeWindowFilter(FeatureFlagFilterModel filter) {
        if (filter.FilterType != FilterTypes.TimeWindow) {
            yield break;
        }

        if (!filter.TimeStart.HasValue && !filter.TimeEnd.HasValue) {
            yield return ValidationResultFor(filter, Flags.ErrorTimeStartOrEndRequired, nameof(filter.TimeStart));
        }

        if (IsEndBeforeStart(filter)) {
            yield return ValidationResultFor(filter, Flags.ErrorTimeWindowEndBeforeStart, nameof(filter.TimeStart));
        }

        if (IsEndEqualStart(filter)) {
            yield return ValidationResultFor(filter, Flags.ErrorTimeWindowEndEqualStart, nameof(filter.TimeStart));
        }

        if (IsRecurrenceIntervalInvalid(filter)) {
            yield return ValidationResultFor(filter, Flags.ErrorRecurrenceIntervalRequired, nameof(filter.TimeRecurrenceInterval));
        }

        if (IsWeeklyRecurrenceDaysMissing(filter)) {
            yield return ValidationResultFor(filter, Flags.ErrorRecurrenceDaysOfWeekRequired, nameof(filter.TimeRecurrenceDaysOfWeek));
        }

        if (IsWeeklyRecurrenceFirstDayMissing(filter)) {
            yield return ValidationResultFor(filter, Flags.ErrorRecurrenceFirstDayOfWeekRequired, nameof(filter.TimeRecurrenceFirstDayOfWeek));
        }

        if (IsEndDateRequired(filter)) {
            yield return ValidationResultFor(filter, Flags.ErrorRecurrenceEndDateRequired, nameof(filter.TimeRecurrenceEndDate));
        }

        if (IsNumberOfOccurrencesRequired(filter)) {
            yield return ValidationResultFor(filter, Flags.ErrorRecurrenceNumberOfOccurrencesRequired, nameof(filter.TimeRecurrenceNumberOfOccurrences));
        }

        // Advanced validation from RecurrenceValidator
        // https://github.com/microsoft/FeatureManagement-Dotnet/blob/main/src/Microsoft.FeatureManagement/FeatureFilters/Recurrence/RecurrenceValidator.cs
        if (filter.TimeRecurrenceType != null) {
            // Max duration: End - Start < 10 years
            if (filter.TimeStart.HasValue && filter.TimeEnd.HasValue && (filter.TimeEnd.Value - filter.TimeStart.Value) >= TimeSpan.FromDays(3650)) {
                yield return ValidationResultFor(filter, Flags.ErrorTimeWindowDuration, nameof(filter.TimeEnd));
            }

            // For daily: duration < interval
            if (filter.TimeRecurrenceType == RecurrencePatternType.Daily && filter.TimeStart.HasValue && filter.TimeEnd.HasValue) {
                var intervalDuration = TimeSpan.FromDays(filter.TimeRecurrenceInterval!.Value);
                var timeWindowDuration = filter.TimeEnd.Value - filter.TimeStart.Value;
                if (timeWindowDuration > intervalDuration) {
                    yield return ValidationResultFor(filter, Flags.ErrorTimeWindowDuration, nameof(filter.TimeEnd));
                }
            }

            // For weekly: duration < interval and duration compliant with days of week
            if (filter.TimeRecurrenceType == RecurrencePatternType.Weekly && filter.TimeStart.HasValue && filter.TimeEnd.HasValue) {
                var intervalDuration = TimeSpan.FromDays(filter.TimeRecurrenceInterval!.Value * _DaysPerWeek);
                var timeWindowDuration = filter.TimeEnd.Value - filter.TimeStart.Value;

                if (timeWindowDuration > intervalDuration ||
                    !IsDurationCompliantWithDaysOfWeek(timeWindowDuration, filter.TimeRecurrenceInterval.Value, filter.TimeRecurrenceDaysOfWeek!, Enum.Parse<DayOfWeek>(filter.TimeRecurrenceFirstDayOfWeek!, true))) {
                    yield return ValidationResultFor(filter, Flags.ErrorTimeWindowDuration, nameof(filter.TimeEnd));
                }

                // Start date must match one of the recurrence days
                if (!filter.TimeRecurrenceDaysOfWeek?.Any(day => Enum.TryParse<DayOfWeek>(day, true, out var dayOfWeek) && dayOfWeek == filter.TimeStart.Value.DayOfWeek) ?? true) {
                    yield return ValidationResultFor(filter, Flags.ErrorStartDateNotValid, nameof(filter.TimeStart));
                }
            }

            // EndDate >= Start
            if (filter.TimeRecurrenceRangeType == RecurrenceRangeType.EndDate && filter.TimeStart.HasValue && filter.TimeRecurrenceEndDate!.Value < filter.TimeStart.Value) {
                yield return ValidationResultFor(filter, Flags.ErrorRecurrrenceEndDateValueOutOfRange, nameof(filter.TimeRecurrenceEndDate));
            }
        }
    }

    private static bool IsEndBeforeStart(FeatureFlagFilterModel filter)
        => filter.TimeStart.HasValue && filter.TimeEnd.HasValue && filter.TimeEnd < filter.TimeStart;

    private static bool IsEndEqualStart(FeatureFlagFilterModel filter)
        => filter.TimeStart.HasValue && filter.TimeEnd.HasValue && filter.TimeEnd == filter.TimeStart;

    private static bool IsRecurrenceIntervalInvalid(FeatureFlagFilterModel filter)
        => filter.TimeRecurrenceType.HasValue && (!filter.TimeRecurrenceInterval.HasValue || filter.TimeRecurrenceInterval <= 0);

    private static bool IsWeeklyRecurrenceDaysMissing(FeatureFlagFilterModel filter)
        => filter.TimeRecurrenceType == RecurrencePatternType.Weekly &&
        (filter.TimeRecurrenceDaysOfWeek == null || !filter.TimeRecurrenceDaysOfWeek.Any());

    private static bool IsWeeklyRecurrenceFirstDayMissing(FeatureFlagFilterModel filter)
        => filter.TimeRecurrenceType == RecurrencePatternType.Weekly && string.IsNullOrWhiteSpace(filter.TimeRecurrenceFirstDayOfWeek);

    private static bool IsEndDateRequired(FeatureFlagFilterModel filter)
        => filter.TimeRecurrenceRangeType == RecurrenceRangeType.EndDate && !filter.TimeRecurrenceEndDate.HasValue;

    private static bool IsNumberOfOccurrencesRequired(FeatureFlagFilterModel filter)
        => filter.TimeRecurrenceRangeType == RecurrenceRangeType.Numbered &&
            (!filter.TimeRecurrenceNumberOfOccurrences.HasValue || filter.TimeRecurrenceNumberOfOccurrences <= 0);


    /// <summary>
    /// Checks whether the duration is shorter than the minimum gap between recurrence of days of week.
    /// </summary>
    /// <param name="duration">The time span of the duration.</param>
    /// <param name="interval">The recurrence interval.</param>
    /// <param name="daysOfWeek">The days of the week when the recurrence will occur.</param>
    /// <param name="firstDayOfWeek">The first day of the week.</param>
    /// <returns>True if the duration is compliant with days of week, false otherwise.</returns>
    private static bool IsDurationCompliantWithDaysOfWeek(TimeSpan duration, int interval, IEnumerable<string> daysOfWeek, DayOfWeek firstDayOfWeek) {
        if (daysOfWeek.Count() == 1) {
            return true;
        }

        var sortedDaysOfWeek = SortDaysOfWeek(daysOfWeek.Select<string, DayOfWeek>(x => Enum.Parse<DayOfWeek>(x, true)), firstDayOfWeek);
        var firstDay = sortedDaysOfWeek[0]; // the closest occurrence day to the first day of week
        var prev = firstDay;
        var minGap = TimeSpan.FromDays(_DaysPerWeek);

        for (var i = 1; i < sortedDaysOfWeek.Count; i++) // start from the second day to calculate the gap
        {
            var dayOfWeek = sortedDaysOfWeek[i];
            var gap = TimeSpan.FromDays(CalculateWeeklyDayOffset(dayOfWeek, prev));

            if (gap < minGap) {
                minGap = gap;
            }

            prev = dayOfWeek;
        }

        // It may across weeks. Check the next week if the interval is one week.
        if (interval == 1) {
            var gap = TimeSpan.FromDays(CalculateWeeklyDayOffset(firstDay, prev));

            if (gap < minGap) {
                minGap = gap;
            }
        }

        return minGap >= duration;
    }

    /// <summary>
    /// Calculates the offset in days between two given days of the week.
    /// <param name="day1">A day of week.</param>
    /// <param name="day2">A day of week.</param>
    /// <returns>The number of days to be added to day2 to reach day1</returns>
    /// </summary>
    private static int CalculateWeeklyDayOffset(DayOfWeek day1, DayOfWeek day2) => ((int)day1 - (int)day2 + _DaysPerWeek) % _DaysPerWeek;

    /// <summary>
    /// Sorts a collection of days of week based on their offsets from a specified first day of week.
    /// <param name="daysOfWeek">A collection of days of week.</param>
    /// <param name="firstDayOfWeek">The first day of week.</param>
    /// <returns>The sorted days of week.</returns>
    /// </summary>
    private static List<DayOfWeek> SortDaysOfWeek(IEnumerable<DayOfWeek> daysOfWeek, DayOfWeek firstDayOfWeek) {
        var result = daysOfWeek.Distinct().ToList(); // dedup

        result.Sort((x, y) =>
            CalculateWeeklyDayOffset(x, firstDayOfWeek)
                .CompareTo(
                    CalculateWeeklyDayOffset(y, firstDayOfWeek)));

        return result;
    }

    private static ValidationResult ValidationResultFor(FeatureFlagFilterModel filter, string error, string property)
        => new(error, [$"Filters[{filter.Index}].{property}"]);

    private static IEnumerable<ValidationResult> ValidatePercentageFilter(FeatureFlagFilterModel filter) {
        if (filter.FilterType != FilterTypes.Percentage) {
            yield break;
        }

        if (!filter.PercentageValue.HasValue || filter.PercentageValue < 0 || filter.PercentageValue > 100) {
            yield return new ValidationResult(Flags.ErrorPercentageOutOfRange, [$"Filters[{filter.Index}].{nameof(filter.PercentageValue)}"]);
        }
    }

    private static IEnumerable<ValidationResult> ValidateJsonFilter(FeatureFlagFilterModel filter) {
        if (filter.FilterType != FilterTypes.JSON) {
            yield break;
        }

        if (string.IsNullOrWhiteSpace(filter.JSON)) {
            yield return new ValidationResult(Flags.ErrorJsonRequired, [$"Filters[{filter.Index}].{nameof(filter.JSON)}"]);
        }

        // add validation of JSON structure - should have a name and parameters property at the root
        ValidationResult? validationResult = null;
        try {
            var configuration = new ConfigurationBuilder()
                .AddJsonStream(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(filter.JSON ?? "")))
                .Build();
            if (configuration == null || string.IsNullOrWhiteSpace(configuration["name"])) {
                validationResult = new ValidationResult(Flags.ErrorJsonInvalidFormat, [$"Filters[{filter.Index}].{nameof(filter.JSON)}"]);
            }
        } catch {
            validationResult = new ValidationResult(Flags.ErrorJsonInvalidFormat, [$"Filters[{filter.Index}].{nameof(filter.JSON)}"]);
        }
        if (validationResult != null) {
            yield return validationResult;
        }
    }
}
