using System.ComponentModel.DataAnnotations;
using FeatureFlags.Constants;
using FeatureFlags.Resources;
using FeatureFlags.Validators;
using Microsoft.FeatureManagement.FeatureFilters;

namespace FeatureFlags.Models;

/// <summary>
/// Represents a system feature flag.
/// </summary>
public sealed record FeatureFlagFilterModel : IAuditedModel {
    /// <summary>
    /// Gets the unique identifier for the filter.
    /// </summary>
    [IsRequired]
    public int Id { get; init; }

    /// <summary>
    /// Gets the type of the filter.
    /// </summary>
    [Display(ResourceType = typeof(Flags), Name = nameof(Flags.FilterType))]
    [IsRequired, IsStringLength(100)]
    public FilterTypes FilterType { get; init; }

    #region Targeting

    /// <summary>
    /// Includes users in the target audience by name.
    /// </summary>
    public List<string>? TargetUsers { get; set; }

    /// <summary>
    /// Includes users in the target audience based off a percentage of the total possible audience. Valid values range from 0 to 100 inclusive.
    /// </summary>
    public double? TargetRolloutPercentage { get; set; }

    /// <summary>
    /// Excludes users in the audience by name.
    /// </summary>
    public List<string>? ExcludeUsers { get; set; }

    #endregion Targeting

    #region TimeWindow

    /// <summary>
    /// An optional start time used to determine when a feature configured to use the <see cref="TimeWindowFilter"/> feature filter should be enabled.
    /// If no start time is specified the time window is considered to have already started.
    /// </summary>
    public DateTimeOffset? TimeStart { get; set; }

    /// <summary>
    /// An optional end time used to determine when a feature configured to use the <see cref="TimeWindowFilter"/> feature filter should be enabled.
    /// If no end time is specified the time window is considered to never end.
    /// </summary>
    public DateTimeOffset? TimeEnd { get; set; }

    /// <summary>
    /// Add-on recurrence rule allows the time window defined by Start and End to recur.
    /// The rule specifies how often the time window repeats.
    /// </summary>
    public RecurrencePatternType? TimeRecurrencePatternType { get; set; }

    /// <summary>
    /// The number of units between occurrences, where units can be in days or weeks, depending on the pattern type.
    /// </summary>
    public int TimeRecurrenceInterval { get; set; } = 1;

    #endregion TimeWindow

    #region Percentage

    /// <summary>
    /// A value between 0 and 100 specifying the chance that a feature configured to use the <see cref="PercentageFilter"/> should be enabled.
    /// </summary>
    public int? PercentageValue { get; set; }

    #endregion

    /// <summary>
    /// Gets the date and time when the feature flag was last updated.
    /// </summary>
    public DateTime UpdatedDate { get; init; }
}
