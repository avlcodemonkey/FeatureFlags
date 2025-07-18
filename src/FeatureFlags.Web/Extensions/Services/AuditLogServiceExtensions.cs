using FeatureFlags.Domain.Models;
using FeatureFlags.Models;
using FeatureFlags.Utils;

namespace FeatureFlags.Extensions.Services;

public static class AuditLogServiceExtensions {
    public static IQueryable<AuditLogModel> SelectAsModel(this IQueryable<AuditLog> query)
        => query.Select(x => new AuditLogModel {
            Id = x.Id, BatchId = x.BatchId, Entity = x.Entity, PrimaryKey = x.PrimaryKey,
            State = x.State, Date = x.Date, OldValues = x.OldValues ?? "", NewValues = x.NewValues ?? "",
            Name = (x.User != null ? x.User.Name : null) ?? "", Email = (x.User != null ? x.User.Email : null) ?? ""
        });

    public static IQueryable<AuditLogSearchResultModel> SelectAsSearchResultModel(this IQueryable<AuditLog> query)
        => query.Select(x => new AuditLogSearchResultModel {
            Id = x.Id, BatchId = x.BatchId, Entity = x.Entity, State = x.State.ToString(),
            UniversalDate = x.Date.ToString("u"),
            Name = NameHelper.DisplayName(x.User == null ? null : x.User.Name, x.User == null ? null : x.User.Email)
        });
}
