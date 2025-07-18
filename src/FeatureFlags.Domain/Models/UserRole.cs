using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FeatureFlags.Domain.Attributes;

namespace FeatureFlags.Domain.Models;

[Table("UserRole")]
public class UserRole : IAuditedEntity {
    [Key]
    public int Id { get; set; }

    public Role Role { get; set; } = null!;

    [Required]
    public int RoleId { get; set; }

    public User User { get; set; } = null!;

    [Required]
    public int UserId { get; set; }

    /// <summary>
    /// When role was created.
    /// </summary>
    /// <remarks>
    /// SQLite specific default value will need to change if backing database is changed.
    /// </remarks>
    [DefaultValueSql("(current_timestamp)")]
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// When role was updated.
    /// </summary>
    /// <remarks>
    /// SQLite specific default value will need to change if backing database is changed.
    /// </remarks>
    [Timestamp, DefaultValueSql("(current_timestamp)")]
    public DateTime UpdatedDate { get; set; }

    [NotMapped]
    public int TemporaryId { get; set; }
}
