using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FeatureFlags.Domain.Attributes;

namespace FeatureFlags.Domain.Models;

[Table("Permission")]
public class Permission : IAuditedEntity {
    public Permission() => RolePermissions = new List<RolePermission>();

    [Key]
    public int Id { get; set; }

    [StringLength(100)]
    [Required]
    public string ActionName { get; set; } = null!;

    [StringLength(100)]
    [Required]
    public string ControllerName { get; set; } = null!;

    [ForeignKey(nameof(RolePermission.PermissionId))]
    public IEnumerable<RolePermission> RolePermissions { get; set; }

    /// <summary>
    /// When permission was created.
    /// </summary>
    /// <remarks>
    /// SQLite specific default value will need to change if backing database is changed.
    /// </remarks>
    [DefaultValueSql("(current_timestamp)")]
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// When permission was updated.
    /// </summary>
    /// <remarks>
    /// Used for concurrency tracking. SQLite specific default value will need to change if backing database is changed.
    /// </remarks>
    [DefaultValueSql("(current_timestamp)")]
    public DateTime UpdatedDate { get; set; }

    [NotMapped]
    public int TemporaryId { get; set; }
}
