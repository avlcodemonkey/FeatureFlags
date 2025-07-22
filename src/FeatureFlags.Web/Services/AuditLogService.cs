using FeatureFlags.Domain;
using FeatureFlags.Extensions.Services;
using FeatureFlags.Models;
using Microsoft.EntityFrameworkCore;

namespace FeatureFlags.Services;

/// <inheritdoc />
public sealed class AuditLogService(FeatureFlagsDbContext dbContext) : IAuditLogService {
    private readonly FeatureFlagsDbContext _DbContext = dbContext;

    /// <summary>
    /// Gets the maximum number of results that can be returned by a query.
    /// </summary>
    public static int MaxResults { get; } = 1000;

    /// <inheritdoc />
    public async Task<IEnumerable<AuditLogSearchResultModel>> SearchLogsAsync(AuditLogSearchModel search, CancellationToken cancellationToken = default) {
        var query = _DbContext.AuditLog.AsNoTracking();

        if (search.StartDate.HasValue) {
            query = query.Where(x => x.Date >= search.StartDate.Value.ToDateTime(new TimeOnly(0)));
        }

        if (search.EndDate.HasValue) {
            query = query.Where(x => x.Date <= search.EndDate.Value.ToDateTime(new TimeOnly(23, 59, 59)));
        }

        if (search.BatchId.HasValue) {
            query = query.Where(x => x.BatchId == search.BatchId);
        }

        if (!string.IsNullOrWhiteSpace(search.Entity)) {
            query = query.Where(x => x.Entity.ToLower() == search.Entity.ToLower());
        }

        if (search.PrimaryKey.HasValue) {
            query = query.Where(x => x.PrimaryKey == search.PrimaryKey);
        }

        if (search.State.HasValue) {
            query = query.Where(x => x.State == search.State);
        }

        if (search.UserId.HasValue) {
            query = query.Where(x => x.UserId == search.UserId);
        }

        query = query.Take(MaxResults + 1);

        return await query.SelectAsSearchResultModel().ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<AuditLogModel?> GetLogByIdAsync(long id, CancellationToken cancellationToken = default)
        => await _DbContext.AuditLog.AsNoTracking().Include(x => x.User).Where(x => x.Id == id).SelectAsModel().FirstOrDefaultAsync(cancellationToken);

    /// <inheritdoc />
    public IEnumerable<EntityState> GetEntityStates()
        => new List<EntityState> { EntityState.Deleted, EntityState.Added, EntityState.Modified };
}
