using FeatureFlags.Models;
using Microsoft.EntityFrameworkCore;

namespace FeatureFlags.Services;

public interface IAuditLogService {
    Task<IEnumerable<AuditLogSearchResultModel>> SearchLogsAsync(AuditLogSearchModel search, CancellationToken cancellationToken = default);

    Task<AuditLogModel?> GetLogByIdAsync(long id, CancellationToken cancellationToken = default);

    IEnumerable<EntityState> GetEntityStates();
}
