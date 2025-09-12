using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FeatureFlags.Domain.Attributes;

namespace FeatureFlags.Domain.Models;

[Table("FeatureFlagFilters")]
public class FeatureFlagFilter : IAuditedEntity {
    /// <summary>
    /// Gets or sets the unique identifier for the entity.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier for the parent feature flag.
    /// </summary>
    [Required]
    public int FeatureFlagId { get; set; }

    /// <summary>
    /// Gets or sets the filter type.
    /// </summary>
    [Required]
    public int FilterType { get; set; }

    #region Targeting

    /// <summary>
    /// Gets or sets the collection of users associated with the feature flag filter.
    /// </summary>
    [ForeignKey(nameof(FeatureFlagFilterUser.FeatureFlagFilterId))]
    public List<FeatureFlagFilterUser> Users { get; set; } = new List<FeatureFlagFilterUser>();

    #endregion Targeting

    #region TimeWindow

    /// <summary>
    /// Gets or sets the start time for the event.
    /// </summary>
    public DateTime? TimeStart { get; set; }

    /// <summary>
    /// Gets or sets the end time of the event.
    /// </summary>
    public DateTime? TimeEnd { get; set; }

    /// <summary>
    /// Gets or sets the type of time recurrence for the event.
    /// </summary>
    public int? TimeRecurrenceType { get; set; }

    /// <summary>
    /// Gets or sets the time recurrence interval, in days or weeks, for a recurring event.
    /// </summary>
    public int? TimeRecurrenceInterval { get; set; }

    /// <summary>
    /// Comma delimited list of days of the week for time recurrence.
    /// </summary>
    [StringLength(100)]
    public string? TimeRecurrenceDaysOfWeek { get; set; }

    /// <summary>
    /// Gets or sets the first day of the week used for time recurrence calculations.
    /// </summary>
    [StringLength(20)]
    public string? TimeRecurrenceFirstDayOfWeek { get; set; }

    /// <summary>
    /// Gets or sets the type of time recurrence range.
    /// </summary>
    public int? TimeRecurrenceRangeType { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the recurrence ends.
    /// </summary>
    public DateTime? TimeRecurrenceEndDate { get; set; }

    /// <summary>
    /// Gets or sets the number of occurrences for a time-based recurrence pattern.
    /// </summary>
    public int? TimeRecurrenceNumberOfOccurrences { get; set; }

    #endregion TimeWindow

    #region Percentage

    /// <summary>
    /// Gets or sets the percentage value. 
    /// </summary>
    public int? PercentageValue { get; set; }

    #endregion Percentage

    #region JSON

    /// <summary>
    /// Gets or sets the filter as JSON-encoded string.
    /// </summary>
    [StringLength(4000)]
    public string? JSON { get; set; }

    #endregion JSON

    /// <summary>
    /// When filter was created.
    /// </summary>
    [DefaultValueSql("(current_timestamp)")]
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// When filter was last updated.
    /// </summary>
    /// <remarks>
    /// SQLite specific default value will need to change if backing database is changed.
    /// </remarks>
    [DefaultValueSql("(current_timestamp)")]
    public DateTime UpdatedDate { get; set; }

    [NotMapped]
    public int TemporaryId { get; set; }
}
