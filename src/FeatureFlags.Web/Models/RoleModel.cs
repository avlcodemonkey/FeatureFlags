using System.ComponentModel.DataAnnotations;
using FeatureFlags.Resources;
using FeatureFlags.Validators;

namespace FeatureFlags.Models;

/// <summary>
/// Represents a role within the feature flag management system.
/// </summary>
public sealed record RoleModel : IAuditedModel, IVersionedModel {
    /// <summary>
    /// Gets the unique identifier for the role.
    /// </summary>
    [IsRequired]
    public int Id { get; init; }

    /// <summary>
    /// Gets the display name of the role.
    /// </summary>
    [Display(ResourceType = typeof(Roles), Name = nameof(Roles.Name))]
    [IsRequired, IsStringLength(100)]
    public string Name { get; init; } = "";

    /// <summary>
    /// Gets a value indicating whether this role is the default role.
    /// </summary>
    [Display(ResourceType = typeof(Roles), Name = nameof(Roles.IsDefault))]
    public bool IsDefault { get; init; } = false;

    /// <summary>
    /// Gets the collection of permission IDs assigned to this role.
    /// </summary>
    [Display(ResourceType = typeof(Roles), Name = nameof(Roles.Permissions))]
    public IEnumerable<int>? PermissionIds { get; init; }

    /// <summary>
    /// Gets the date and time when the role was last updated.
    /// </summary>
    public DateTime UpdatedDate { get; init; }
}
