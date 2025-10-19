using System.ComponentModel.DataAnnotations;
using FeatureFlags.Client;
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
    [IsRequired]
    public FilterTypes FilterType { get; init; }

    #region Targeting

    /// <summary>
    /// Includes users in the target audience by name.
    /// </summary>
    [Display(ResourceType = typeof(Flags), Name = nameof(Flags.TargetUsers))]
    public IEnumerable<string>? TargetUsers { get; set; }

    /// <summary>
    /// Excludes users in the audience by name.
    /// </summary>
    [Display(ResourceType = typeof(Flags), Name = nameof(Flags.ExcludeUsers))]
    public IEnumerable<string>? ExcludeUsers { get; set; }

    #endregion Targeting

    #region TimeWindow

    /// <summary>
    /// An optional start time used to determine when a feature configured to use the <see cref="TimeWindowFilter"/> feature filter should be enabled.
    /// If no start time is specified the time window is considered to have already started.
    /// </summary>
    /// <remarks>Will always be provided in UTC.</remarks>
    [Display(ResourceType = typeof(Flags), Name = nameof(Flags.TimeStart))]
    public DateTime? TimeStart { get; set; }

    /// <summary>
    /// An optional end time used to determine when a feature configured to use the <see cref="TimeWindowFilter"/> feature filter should be enabled.
    /// If no end time is specified the time window is considered to never end.
    /// </summary>
    /// <remarks>Will always be provided in UTC.</remarks>
    [Display(ResourceType = typeof(Flags), Name = nameof(Flags.TimeEnd))]
    public DateTime? TimeEnd { get; set; }

    /// <summary>
    /// Add-on recurrence rule allows the time window defined by Start and End to recur.
    /// The rule specifies how often the time window repeats.
    /// </summary>
    [Display(ResourceType = typeof(Flags), Name = nameof(Flags.TimeRecurrenceType))]
    public RecurrencePatternType? TimeRecurrenceType { get; set; }

    /// <summary>
    /// Specifies the number of days/weeks between each occurrence.
    /// </summary>
    [Display(ResourceType = typeof(Flags), Name = nameof(Flags.TimeRecurrenceInterval))]
    public int? TimeRecurrenceInterval { get; set; }

    /// <summary>
    /// Specifies on which days of the week the event occurs.
    /// </summary>
    [Display(ResourceType = typeof(Flags), Name = nameof(Flags.TimeRecurrenceDaysOfWeek))]
    public IEnumerable<string>? TimeRecurrenceDaysOfWeek { get; set; }

    /// <summary>
    /// Specifies which day is considered the first day of the week.
    /// </summary>
    [Display(ResourceType = typeof(Flags), Name = nameof(Flags.TimeRecurrenceFirstDayOfWeek))]
    public string? TimeRecurrenceFirstDayOfWeek { get; set; }

    /// <summary>
    /// Specifies how the recurrence ends.
    /// </summary>
    [Display(ResourceType = typeof(Flags), Name = nameof(Flags.TimeRecurrenceRangeType))]
    public RecurrenceRangeType? TimeRecurrenceRangeType { get; set; }

    /// <summary>
    /// Specifies the end date for the event.
    /// </summary>
    /// <remarks>Will always be provided in UTC.</remarks>
    [Display(ResourceType = typeof(Flags), Name = nameof(Flags.TimeRecurrenceEndDate))]
    public DateTime? TimeRecurrenceEndDate { get; set; }

    /// <summary>
    /// Specifies the number of occurrences for the event to occur.
    /// </summary>
    [Display(ResourceType = typeof(Flags), Name = nameof(Flags.TimeRecurrenceNumberOfOccurrences))]
    public int? TimeRecurrenceNumberOfOccurrences { get; set; }

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
    /// Unique index to use for rendering the UI.
    /// </summary>
    public string Index {
        get => _Index ??= (Id > 0 ? Id.ToString() : FilterConstants.IndexPlaceholder);
        set => _Index = value;
    }
    private string? _Index;

    /// <summary>
    /// Gets the date and time when the feature flag was last updated.
    /// </summary>
    public DateTime UpdatedDate { get; init; }
}
