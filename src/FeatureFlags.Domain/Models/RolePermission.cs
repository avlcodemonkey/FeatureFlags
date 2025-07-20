using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FeatureFlags.Domain.Attributes;

namespace FeatureFlags.Domain.Models;

[Table("RolePermission")]
public class RolePermission : IAuditedEntity {
    [Key]
    public int Id { get; set; }

    public Permission Permission { get; set; } = null!;

    [Required]
    public int PermissionId { get; set; }

    public Role Role { get; set; } = null!;

    [Required]
    public int RoleId { get; set; }

    /// <summary>
    /// When role permission was created.
    /// </summary>
    /// <remarks>
    /// SQLite specific default value will need to change if backing database is changed.
    /// </remarks>
    [DefaultValueSql("(current_timestamp)")]
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// When role permission was updated.
    /// </summary>
    /// <remarks>
    /// Used for concurrency tracking. SQLite specific default value will need to change if backing database is changed.
    /// </remarks>
    [DefaultValueSql("(current_timestamp)")]
    public DateTime UpdatedDate { get; set; }

    [NotMapped]
    public int TemporaryId { get; set; }
}
