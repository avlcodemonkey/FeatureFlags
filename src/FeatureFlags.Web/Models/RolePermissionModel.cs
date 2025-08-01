using FeatureFlags.Validators;

namespace FeatureFlags.Models;

/// <summary>
/// Represents the association between a role and a permission within the system.
/// </summary>
public sealed record RolePermissionModel : IAuditedModel, IEquatable<RolePermissionModel> {
    /// <summary>
    /// Gets the unique identifier for the entity.
    /// </summary>
    [IsRequired]
    public int Id { get; init; }

    /// <summary>
    /// Gets the unique identifier for the permission.
    /// </summary>
    [IsRequired]
    public int PermissionId { get; init; }

    /// <summary>
    /// Gets the unique identifier for the role.
    /// </summary>
    [IsRequired]
    public int RoleId { get; init; }
}
