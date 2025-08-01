namespace FeatureFlags.Models;

/// <summary>
/// Represents a language that the application can be translated into. Only used for display purposes, so no model validation needed.
/// </summary>
public sealed record LanguageModel : IAuditedModel {
    /// <summary>
    /// Gets the unique identifier for the language.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Gets the language code (e.g., "en", "es").
    /// </summary>
    public string LanguageCode { get; init; } = "";

    /// <summary>
    /// Gets the display name of the language.
    /// </summary>
    public string Name { get; init; } = "";

    /// <summary>
    /// Gets a value indicating whether this language is the default.
    /// </summary>
    public bool IsDefault { get; init; }
}
