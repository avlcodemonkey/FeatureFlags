using System.ComponentModel.DataAnnotations;
using FeatureFlags.Resources;
using FeatureFlags.Validators;

namespace FeatureFlags.Models;

/// <summary>
/// Model representing the data required to copy a role.
/// </summary>
public sealed record CopyRoleModel : IAuditedModel {
    /// <summary>
    /// Gets the unique identifier for the role to be copied.
    /// </summary>
    [IsRequired]
    public int Id { get; init; }

    /// <summary>
    /// Gets the prompt or new name for the copied role.
    /// </summary>
    [Display(ResourceType = typeof(Roles), Name = nameof(Roles.NewName))]
    [IsRequired, IsStringLength(100)]
    public string Prompt { get; init; } = "";
}
