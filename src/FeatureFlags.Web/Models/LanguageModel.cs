namespace FeatureFlags.Models;

/// <summary>
/// Represents a language that the application can be translated into. Only used for display purposes, so no model validation needed.
/// </summary>
public sealed record LanguageModel : IAuditedModel {
    public int Id { get; init; }

    public string LanguageCode { get; init; } = "";

    public string Name { get; init; } = "";

    public bool IsDefault { get; init; }
}
