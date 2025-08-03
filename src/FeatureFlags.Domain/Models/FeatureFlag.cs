using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FeatureFlags.Domain.Attributes;
using Microsoft.EntityFrameworkCore;

namespace FeatureFlags.Domain.Models;

[Table("FeatureFlag")]
[Index(nameof(Name), IsUnique = true)]
public class FeatureFlag : IAuditedEntity, IVersionedEntity {
    [Key]
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = null!;

    public bool IsEnabled { get; set; }

    /// <summary>
    /// Gets or sets the type of requirement associated with the current entity.
    /// </summary>
    /// <remarks>
    /// Maps to the <see cref="FeatureFlags.Web.Constants.RequirementType"/> enum.
    /// </remarks>
    public int RequirementType { get; set; }

    /// <summary>
    /// When flag was created.
    /// </summary>
    /// <remarks>
    /// SQLite specific default value will need to change if backing database is changed.
    /// </remarks>
    [DefaultValueSql("(current_timestamp)")]
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// When flag was last updated.
    /// </summary>
    /// <remarks>
    /// Used for concurrency tracking. SQLite specific default value will need to change if backing database is changed.
    /// </remarks>
    [DefaultValueSql("(current_timestamp)")]
    public DateTime UpdatedDate { get; set; }

    [NotMapped]
    public int TemporaryId { get; set; }
}
