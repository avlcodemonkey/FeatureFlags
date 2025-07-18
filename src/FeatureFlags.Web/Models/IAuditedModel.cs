namespace FeatureFlags.Models;

/// <summary>
/// Fields needed for auditing.
/// </summary>
/// <remarks>FeatureFlags.Domain.Models.IAuditedEntity has additional fields needed for EF but not currently used client side.</remarks>
public interface IAuditedModel {
    /// <summary>
    /// Database incremented unique identifier for this model.
    /// </summary>
    int Id { get; init; }
}
