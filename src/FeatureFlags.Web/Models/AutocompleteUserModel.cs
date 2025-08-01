namespace FeatureFlags.Models;

/// <summary>
/// Used for returning search results for the audit log user autocomplete.
/// </summary>
public sealed record AutocompleteUserModel {
    /// <summary>
    /// Gets the display label for the user.
    /// </summary>
    public string Label { get; init; } = "";

    /// <summary>
    /// Gets the unique identifier value for the user.
    /// </summary>
    public int Value { get; init; }
}
