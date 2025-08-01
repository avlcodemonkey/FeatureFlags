using FeatureFlags.Domain.Models;
using FeatureFlags.Models;
using FeatureFlags.Utils;

namespace FeatureFlags.Extensions.Services;

/// <summary>
/// Provides extension methods for transforming <see cref="IQueryable{AuditLog}"/> objects into specific models.
/// </summary>
public static class AuditLogServiceExtensions {
    /// <summary>
    /// Projects a sequence of <see cref="AuditLog"/> entities into a sequence of <see cref="AuditLogModel"/> objects.
    /// </summary>
    /// <param name="query"><see cref="IQueryable{T}"/> sequence of <see cref="AuditLog"/> entities to project.</param>
    /// <returns><see cref="IQueryable{T}"/> sequence of <see cref="AuditLogModel"/> objects.</returns>
    public static IQueryable<AuditLogModel> SelectAsModel(this IQueryable<AuditLog> query)
        => query.Select(x => new AuditLogModel {
            Id = x.Id, BatchId = x.BatchId, Entity = x.Entity, PrimaryKey = x.PrimaryKey,
            State = x.State, Date = x.Date, OldValues = x.OldValues ?? "", NewValues = x.NewValues ?? "",
            Name = (x.User != null ? x.User.Name : null) ?? "", Email = (x.User != null ? x.User.Email : null) ?? ""
        });

    /// <summary>
    /// Projects an <see cref="IQueryable{AuditLog}"/> into an <see cref="IQueryable{AuditLogSearchResultModel}"/>.
    /// </summary>
    /// <param name="query">Source query of <see cref="AuditLog"/> objects to be transformed.</param>
    /// <returns><see cref="IQueryable{AuditLogSearchResultModel}"/> containing the projected data from the source query.</returns>
    public static IQueryable<AuditLogSearchResultModel> SelectAsSearchResultModel(this IQueryable<AuditLog> query)
        => query.Select(x => new AuditLogSearchResultModel {
            Id = x.Id, BatchId = x.BatchId, Entity = x.Entity, State = x.State.ToString(),
            UniversalDate = x.Date.ToString("u"),
            Name = NameHelper.DisplayName(x.User == null ? null : x.User.Name, x.User == null ? null : x.User.Email)
        });
}
