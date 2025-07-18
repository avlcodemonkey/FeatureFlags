namespace FeatureFlags.Models;

/// <summary>
/// Subset of User used only for showing the user list.
/// </summary>
public sealed record UserListResultModel : IAuditedModel {
    public int Id { get; init; }

    public string Name { get; init; } = "";

    public string Email { get; init; } = "";
}
