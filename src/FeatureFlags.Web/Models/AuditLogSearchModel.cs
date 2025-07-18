using System.ComponentModel.DataAnnotations;
using FeatureFlags.Resources;
using Microsoft.EntityFrameworkCore;

namespace FeatureFlags.Models;

/// <summary>
/// Used to group search params and pass them around.
/// </summary>
public sealed record AuditLogSearchModel {
    [Display(ResourceType = typeof(AuditLogs), Name = nameof(AuditLogs.Batch))]
    public Guid? BatchId { get; set; }

    [Display(ResourceType = typeof(AuditLogs), Name = nameof(AuditLogs.Entity))]
    public string? Entity { get; set; }

    [Display(ResourceType = typeof(AuditLogs), Name = nameof(AuditLogs.PrimaryKey))]
    public long? PrimaryKey { get; set; }

    /// <summary>
    /// Should only be one of: Deleted = 2, Modified = 3, Added = 4
    /// </summary>
    [Display(ResourceType = typeof(AuditLogs), Name = nameof(AuditLogs.State))]
    public EntityState? State { get; set; }

    [Display(ResourceType = typeof(AuditLogs), Name = nameof(AuditLogs.StartDate))]
    public DateOnly? StartDate { get; set; } = DateOnly.FromDateTime(DateTime.Now.AddDays(-30));

    [Display(ResourceType = typeof(AuditLogs), Name = nameof(AuditLogs.EndDate))]
    public DateOnly? EndDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);

    [Display(ResourceType = typeof(AuditLogs), Name = nameof(AuditLogs.Name))]
    public int? UserId { get; set; }
}
