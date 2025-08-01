namespace FeatureFlags.Models;

/// <summary>
/// Subset of User used only for showing the user list.
/// </summary>
public sealed record UserListResultModel : IAuditedModel {
    /// <summary>
    /// Gets the unique identifier for the entity.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Gets the name associated with the current object.
    /// </summary>
    public string Name { get; init; } = "";

    /// <summary>
    /// Gets the email address associated with the entity.
    /// </summary>
    public string Email { get; init; } = "";
}
