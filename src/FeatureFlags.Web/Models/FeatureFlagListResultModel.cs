using FeatureFlags.Resources;

namespace FeatureFlags.Models;

/// <summary>
/// Subset of FeatureFlag used only for showing the feature flag list.
/// </summary>
public sealed record FeatureFlagListResultModel : IAuditedModel {
    public int Id { get; init; }

    public string Name { get; init; } = "";

    public bool IsEnabled { get; init; }

    public string StatusText => IsEnabled ? Flags.Enabled : Flags.Disabled;
}
