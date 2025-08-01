namespace FeatureFlags.Models;

/// <summary>
/// Represents an action method the user could execute. Not editable by users, so no model validation needed.
/// </summary>
public sealed record PermissionModel : IAuditedModel {
    /// <summary>
    /// Gets the unique identifier for the permission.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Gets the name of the action associated with the permission.
    /// </summary>
    public string ActionName { get; init; } = "";

    /// <summary>
    /// Gets the name of the controller associated with the permission.
    /// </summary>
    public string ControllerName { get; init; } = "";
}
