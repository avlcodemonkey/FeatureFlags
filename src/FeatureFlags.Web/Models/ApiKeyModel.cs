using System.ComponentModel.DataAnnotations;
using FeatureFlags.Resources;
using FeatureFlags.Validators;

namespace FeatureFlags.Models;

/// <summary>
/// Represents an api key.
/// </summary>
public sealed record ApiKeyModel : IAuditedModel {
    /// <summary>
    /// Gets the unique identifier for the entity.
    /// </summary>
    [IsRequired]
    public int Id { get; init; }

    /// <summary>
    /// Gets the name associated with the API key.
    /// </summary>
    [Display(ResourceType = typeof(ApiKeys), Name = nameof(ApiKeys.Name))]
    [IsRequired, IsStringLength(100)]
    public string Name { get; init; } = "";

    /// <summary>
    /// Gets the API key used for authentication.
    /// </summary>
    [Display(ResourceType = typeof(ApiKeys), Name = nameof(ApiKeys.Key))]
    [IsRequired, IsStringLength(100)]
    public string Key { get; init; } = "";

    /// <summary>
    /// Gets the date and time when the entity was created.
    /// </summary>
    public DateTime CreatedDate { get; init; }

    /// <summary>
    /// Gets the date and time when the entity was last updated.
    /// </summary>
    public DateTime UpdatedDate { get; init; }
}
