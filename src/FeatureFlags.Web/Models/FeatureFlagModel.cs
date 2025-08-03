using System.ComponentModel.DataAnnotations;
using FeatureFlags.Constants;
using FeatureFlags.Resources;
using FeatureFlags.Validators;

namespace FeatureFlags.Models;

/// <summary>
/// Represents a system feature flag.
/// </summary>
public sealed record FeatureFlagModel : IAuditedModel, IVersionedModel {
    /// <summary>
    /// Gets the unique identifier for the feature flag.
    /// </summary>
    [IsRequired]
    public int Id { get; init; }

    /// <summary>
    /// Gets the display name of the feature flag.
    /// </summary>
    [Display(ResourceType = typeof(Flags), Name = nameof(Flags.Name))]
    [IsRequired, IsStringLength(100)]
    public string Name { get; init; } = "";

    /// <summary>
    /// Gets a value indicating whether the feature flag is active.
    /// </summary>
    [Display(ResourceType = typeof(Flags), Name = nameof(Flags.Active))]
    public bool Status { get; init; }

    /// <summary>
    /// Gets a value indicating whether any or all requirements must be met.
    /// </summary>
    [Display(ResourceType = typeof(Flags), Name = nameof(Flags.RequirementType))]
    [IsRequired]
    public RequirementType RequirementType { get; init; }

    /// <summary>
    /// Gets the date and time when the feature flag was last updated.
    /// </summary>
    public DateTime UpdatedDate { get; init; }
}
