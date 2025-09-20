using FeatureFlags.Resources;

namespace FeatureFlags.Models;

/// <summary>
/// Subset of FeatureFlag used only for showing the feature flag list.
/// </summary>
public sealed record FeatureFlagListResultModel : IAuditedModel {
    /// <summary>
    /// Gets the unique identifier for the feature flag.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Gets the display name of the feature flag.
    /// </summary>
    public string Name { get; init; } = "";

    /// <summary>
    /// Gets a value indicating whether the feature flag is active.
    /// </summary>
    public bool Status { get; init; }

    /// <summary>
    /// Gets the localized status text for the feature flag.
    /// </summary>
    public string StatusText => Status ? Flags.Active : Flags.Inactive;

    /// <summary>
    /// Gets the enabled text for the feature flag.
    /// </summary>
    public string EvaluationText { get; init; } = "";
}
