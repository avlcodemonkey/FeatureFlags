using FeatureFlags.Validators;

namespace FeatureFlags.Models;

/// <summary>
/// Represents the association between a user and a role within the system.
/// </summary>
public sealed record UserRoleModel : IAuditedModel {
    /// <summary>
    /// Gets the unique identifier for the entity.
    /// </summary>
    [IsRequired]
    public int Id { get; init; }

    /// <summary>
    /// Gets the unique identifier for the role associated with the entity.
    /// </summary>
    [IsRequired]
    public int RoleId { get; init; }

    /// <summary>
    /// Gets the unique identifier for the user.
    /// </summary>
    [IsRequired]
    public int UserId { get; init; }
}
