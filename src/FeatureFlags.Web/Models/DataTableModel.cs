namespace FeatureFlags.Models;

/// <summary>
/// Model representing options for a data table display.
/// </summary>
public sealed record DataTableModel {
    /// <summary>
    /// Gets a value indicating whether the search functionality should be hidden.
    /// </summary>
    public bool HideSearch { get; init; } = false;
}
