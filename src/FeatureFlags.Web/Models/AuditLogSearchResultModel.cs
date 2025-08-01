namespace FeatureFlags.Models;

/// <summary>
/// Subset of AuditLog used only for showing search results.
/// </summary>
public sealed record AuditLogSearchResultModel {
    /// <summary>
    /// Gets the unique identifier for the audit log entry.
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// Gets the batch identifier associated with the audit log entry.
    /// </summary>
    public Guid BatchId { get; init; }

    /// <summary>
    /// Gets the name of the entity affected by the audit log entry.
    /// </summary>
    public string Entity { get; init; } = "";

    /// <summary>
    /// Gets the state or action recorded in the audit log entry.
    /// </summary>
    public string State { get; init; } = "";

    /// <summary>
    /// Gets the universal date of the audit log entry as a string.
    /// </summary>
    public string UniversalDate { get; init; } = "";

    /// <summary>
    /// Gets the display name associated with the audit log entry.
    /// </summary>
    public string Name { get; init; } = "";
}
