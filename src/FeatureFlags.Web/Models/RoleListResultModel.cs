namespace FeatureFlags.Models;

/// <summary>
/// Subset of Role used only for showing the role list.
/// </summary>
public sealed record RoleListResultModel : IAuditedModel {
    /// <summary>
    /// Gets the unique identifier for the role.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Gets the display name of the role.
    /// </summary>
    public string Name { get; init; } = "";
}
