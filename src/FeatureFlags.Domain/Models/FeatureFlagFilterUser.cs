using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FeatureFlags.Domain.Attributes;

namespace FeatureFlags.Domain.Models;

[Table("FeatureFlagFilterUsers")]
public class FeatureFlagFilterUser : IAuditedEntity {
    /// <summary>
    /// Gets or sets the unique identifier for the entity.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier for the parent feature flag filter.
    /// </summary>
    [Required]
    public int FeatureFlagFilterId { get; set; }

    /// <summary>
    /// Gets or sets the name of the user.
    /// </summary>
    [Required]
    [StringLength(100)]
    public string User { get; set; } = "";

    /// <summary>
    /// Include or exclude user.
    /// </summary>
    [Required]
    public bool Include { get; set; }

    /// <summary>
    /// When user was created.
    /// </summary>
    [DefaultValueSql("(current_timestamp)")]
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// When user was last updated.
    /// </summary>
    /// <remarks>
    /// SQLite specific default value will need to change if backing database is changed.
    /// </remarks>
    [DefaultValueSql("(current_timestamp)")]
    public DateTime UpdatedDate { get; set; }

    [NotMapped]
    public int TemporaryId { get; set; }
}
