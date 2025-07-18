using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FeatureFlags.Domain.Attributes;
using Microsoft.EntityFrameworkCore;

namespace FeatureFlags.Domain.Models;

[Table("User")]
[Index(nameof(Email), IsUnique = true)]
public class User : IAuditedEntity, IVersionedEntity {
    [Key]
    public int Id { get; set; }

    [StringLength(100)]
    [Required]
    public string Email { get; set; } = null!;

    [StringLength(100)]
    [Required]
    public string Name { get; set; } = null!;

    [Required]
    public int LanguageId { get; set; }

    [DefaultValue(true)]
    public bool Status { get; set; }

    [ForeignKey(nameof(UserRole.UserId))]
    public IEnumerable<UserRole> UserRoles { get; set; } = new List<UserRole>();

    /// <summary>
    /// When user was created.
    /// </summary>
    /// <remarks>
    /// SQLite specific default value will need to change if backing database is changed.
    /// </remarks>
    [DefaultValueSql("(current_timestamp)")]
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// When user was updated.
    /// </summary>
    /// <remarks>
    /// Used for concurrency tracking. SQLite specific default value will need to change if backing database is changed.
    /// </remarks>
    [Timestamp, DefaultValueSql("(current_timestamp)")]
    public DateTime UpdatedDate { get; set; }

    [NotMapped]
    public int TemporaryId { get; set; }
}
