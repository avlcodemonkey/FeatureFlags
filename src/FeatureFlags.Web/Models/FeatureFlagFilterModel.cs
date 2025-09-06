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
    [Display(ResourceType = typeof(Flags), Name = nameof(Flags.TargetUsers))]
    public List<string>? TargetUsers { get; set; }

    /// <summary>
    /// Excludes users in the audience by name.
    /// </summary>
    [Display(ResourceType = typeof(Flags), Name = nameof(Flags.ExcludeUsers))]
    public List<string>? ExcludeUsers { get; set; }

    #endregion Targeting

    #region TimeWindow

    /// <summary>
    /// An optional start time used to determine when a feature configured to use the <see cref="TimeWindowFilter"/> feature filter should be enabled.
    /// If no start time is specified the time window is considered to have already started.
    /// </summary>
    [Display(ResourceType = typeof(Flags), Name = nameof(Flags.TimeStart))]
    public DateTimeOffset? TimeStart { get; set; }

    /// <summary>
    /// An optional end time used to determine when a feature configured to use the <see cref="TimeWindowFilter"/> feature filter should be enabled.
    /// If no end time is specified the time window is considered to never end.
    /// </summary>
    [Display(ResourceType = typeof(Flags), Name = nameof(Flags.TimeEnd))]
    public DateTimeOffset? TimeEnd { get; set; }

    /// <summary>
    /// Add-on recurrence rule allows the time window defined by Start and End to recur.
    /// The rule specifies how often the time window repeats.
    /// </summary>
    [Display(ResourceType = typeof(Flags), Name = nameof(Flags.RecurrencePattern))]
    public RecurrencePatternType? TimeRecurrencePatternType { get; set; }

    #endregion TimeWindow

    #region Percentage

    /// <summary>
    /// A value between 0 and 100 specifying the chance that a feature configured to use the <see cref="PercentageFilter"/> should be enabled.
    /// </summary>
    [Display(ResourceType = typeof(Flags), Name = nameof(Flags.Percentage))]
    public int? PercentageValue { get; set; }

    #endregion Percentage

    #region JSON

    /// <summary>
    /// A JSON filter specification.
    /// </summary>
    [Display(ResourceType = typeof(Flags), Name = nameof(Flags.JSON))]
    public string? JSON { get; set; }

    #endregion JSON

    /// <summary>
    /// Gets the date and time when the feature flag was last updated.
    /// </summary>
    public DateTime UpdatedDate { get; init; }
}
