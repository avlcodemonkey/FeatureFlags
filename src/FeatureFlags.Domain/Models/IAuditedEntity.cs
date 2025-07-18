namespace FeatureFlags.Domain.Models;

/// <summary>
/// Fields needed for change logging.
/// </summary>
public interface IAuditedEntity {
    int Id { get; set; }

    DateTime CreatedDate { get; set; }

    DateTime UpdatedDate { get; set; }

    int TemporaryId { get; set; }
}
