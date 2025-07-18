using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FeatureFlags.Domain.Attributes;
using Microsoft.EntityFrameworkCore;

namespace FeatureFlags.Domain.Models;

[Table("Role")]
[Index(nameof(Name), IsUnique = true)]
public class Role : IAuditedEntity, IVersionedEntity {
    [Key]
    public int Id { get; set; }

    public bool IsDefault { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = null!;

    [ForeignKey(nameof(RolePermission.RoleId))]
    public List<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

    [ForeignKey(nameof(UserRole.RoleId))]
    public List<UserRole> UserRoles { get; set; } = new List<UserRole>();

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
    /// Used for concurrency tracking. SQLite specific default value will need to change if backing database is changed.
    /// </remarks>
    [DefaultValueSql("(current_timestamp)")]
    public DateTime UpdatedDate { get; set; }

    [NotMapped]
    public int TemporaryId { get; set; }
}
