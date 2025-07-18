using Microsoft.EntityFrameworkCore;

namespace FeatureFlags.Models;

/// <summary>
/// Represents an audit log entry for tracking change history. Only used for display purposes, so no model validation needed.
/// </summary>
/// <remarks>No need for for validation or display attributes for this model.</remarks>
public sealed record AuditLogModel {
    public long Id { get; init; }

    public Guid BatchId { get; init; }

    public string Entity { get; init; } = "";

    public long PrimaryKey { get; init; }

    /// <summary>
    /// Should only be one of: Deleted = 2, Modified = 3, Added = 4
    /// </summary>
    public EntityState State { get; init; }

    public DateTime Date { get; init; }

    public string Name { get; init; } = "";

    public string Email { get; init; } = "";

    public string OldValues { get; init; } = "";

    public string NewValues { get; init; } = "";
}
