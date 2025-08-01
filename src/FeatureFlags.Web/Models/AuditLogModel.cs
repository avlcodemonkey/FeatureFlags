using Microsoft.EntityFrameworkCore;

namespace FeatureFlags.Models;

/// <summary>
/// Represents an audit log entry for tracking change history. Only used for display purposes, so no model validation needed.
/// </summary>
public sealed record AuditLogModel {
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
    /// Gets the primary key value of the entity affected by the audit log entry.
    /// </summary>
    public long PrimaryKey { get; init; }

    /// <summary>
    /// Gets the state of the entity. Should only be one of: Deleted = 2, Modified = 3, Added = 4.
    /// </summary>
    public EntityState State { get; init; }

    /// <summary>
    /// Gets the date and time of the audit log entry.
    /// </summary>
    public DateTime Date { get; init; }

    /// <summary>
    /// Gets the display name associated with the audit log entry.
    /// </summary>
    public string Name { get; init; } = "";

    /// <summary>
    /// Gets the email address associated with the audit log entry.
    /// </summary>
    public string Email { get; init; } = "";

    /// <summary>
    /// Gets the old values before the change, serialized as a string.
    /// </summary>
    public string OldValues { get; init; } = "";

    /// <summary>
    /// Gets the new values after the change, serialized as a string.
    /// </summary>
    public string NewValues { get; init; } = "";
}
