using FeatureFlags.Constants;

namespace FeatureFlags.Models;

/// <summary>
/// Model representing an alert message to display to the user.
/// </summary>
public sealed record AlertModel {
    /// <summary>
    /// Gets the content of the alert message.
    /// </summary>
    public string Content { get; init; } = "";

    /// <summary>
    /// Gets the type of alert (e.g., success or error).
    /// </summary>
    public AlertType AlertType { get; init; } = AlertType.Success;

    /// <summary>
    /// Gets a value indicating whether the alert can be dismissed by the user.
    /// </summary>
    public bool CanDismiss { get; init; } = true;

    /// <summary>
    /// Gets the icon to display with the alert, if any.
    /// </summary>
    public Icon? Icon { get; init; }
}
