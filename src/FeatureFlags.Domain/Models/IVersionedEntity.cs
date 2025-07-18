namespace FeatureFlags.Domain.Models;

/// <summary>
/// Fields needed for concurrency with versioned entities.
/// </summary>
public interface IVersionedEntity {
    /// <summary>
    /// Timestamp for tracking the version of the entity.
    /// </summary>
    DateTime UpdatedDate { get; set; }
}
