namespace FeatureFlags.Models;

/// <summary>
/// Subset of Role used only for showing the role list.
/// </summary>
public sealed record RoleListResultModel : IAuditedModel {
    public int Id { get; init; }

    public string Name { get; init; } = "";
}
