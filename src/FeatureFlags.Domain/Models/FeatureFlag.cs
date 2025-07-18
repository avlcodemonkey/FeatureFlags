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

    [StringLength(100)]
    [Required]
    public string Name { get; set; } = null!;

    public bool IsEnabled { get; set; }

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
