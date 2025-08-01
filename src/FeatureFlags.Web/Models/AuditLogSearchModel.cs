using System.ComponentModel.DataAnnotations;
using FeatureFlags.Resources;
using Microsoft.EntityFrameworkCore;

namespace FeatureFlags.Models;

/// <summary>
/// Used to group search params and pass them around.
/// </summary>
public sealed record AuditLogSearchModel {
    /// <summary>
    /// Gets or sets the batch identifier to filter audit logs.
    /// </summary>
    [Display(ResourceType = typeof(AuditLogs), Name = nameof(AuditLogs.Batch))]
    public Guid? BatchId { get; set; }

    /// <summary>
    /// Gets or sets the entity name to filter audit logs.
    /// </summary>
    [Display(ResourceType = typeof(AuditLogs), Name = nameof(AuditLogs.Entity))]
    public string? Entity { get; set; }

    /// <summary>
    /// Gets or sets the primary key value to filter audit logs.
    /// </summary>
    [Display(ResourceType = typeof(AuditLogs), Name = nameof(AuditLogs.PrimaryKey))]
    public long? PrimaryKey { get; set; }

    /// <summary>
    /// Gets or sets the state of the entity. Should only be one of: Deleted = 2, Modified = 3, Added = 4.
    /// </summary>
    [Display(ResourceType = typeof(AuditLogs), Name = nameof(AuditLogs.State))]
    public EntityState? State { get; set; }

    /// <summary>
    /// Gets or sets the start date for the audit log search range.
    /// </summary>
    [Display(ResourceType = typeof(AuditLogs), Name = nameof(AuditLogs.StartDate))]
    public DateOnly? StartDate { get; set; } = DateOnly.FromDateTime(DateTime.Now.AddDays(-30));

    /// <summary>
    /// Gets or sets the end date for the audit log search range.
    /// </summary>
    [Display(ResourceType = typeof(AuditLogs), Name = nameof(AuditLogs.EndDate))]
    public DateOnly? EndDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);

    /// <summary>
    /// Gets or sets the user identifier to filter audit logs.
    /// </summary>
    [Display(ResourceType = typeof(AuditLogs), Name = nameof(AuditLogs.Name))]
    public int? UserId { get; set; }
}
