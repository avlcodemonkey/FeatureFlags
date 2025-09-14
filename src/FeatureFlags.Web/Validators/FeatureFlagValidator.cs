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
    }

    private static bool IsEndBeforeStart(FeatureFlagFilterModel filter)
        => filter.TimeStart.HasValue && filter.TimeEnd.HasValue && filter.TimeEnd < filter.TimeStart;

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
    }
}
