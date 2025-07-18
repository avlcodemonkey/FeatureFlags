namespace FeatureFlags.Models;

/// <summary>
/// Used for returning search results for the audit log user autocomplete.
/// </summary>
public sealed record AutocompleteUserModel {
    public string Label { get; init; } = "";

    public int Value { get; init; }
}
