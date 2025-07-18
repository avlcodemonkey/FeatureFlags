using System.ComponentModel.DataAnnotations;
using FeatureFlags.Resources;
using FeatureFlags.Validators;

namespace FeatureFlags.Models;

/// <summary>
/// Represents a system feature flag.
/// </summary>
public sealed record FeatureFlagModel : IAuditedModel {
    [IsRequired]
    public int Id { get; init; }

    [Display(ResourceType = typeof(Flags), Name = nameof(Flags.Name))]
    [IsRequired, IsStringLength(100)]
    public string Name { get; init; } = "";

    public bool IsEnabled { get; init; }

    public DateTime UpdatedDate { get; init; }

}
