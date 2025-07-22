using FeatureFlags.Models;
using Microsoft.EntityFrameworkCore;

namespace FeatureFlags.Services;

/// <summary>
/// Provides functionality to interact with audit logs, including searching and retrieving logs by ID.
/// </summary>
public interface IAuditLogService {
    /// <summary>
    /// Asynchronously searches audit logs based on specified criteria.
    /// </summary>
    /// <remarks>The search results are limited to a maximum number of entries as defined by the
    /// <c>MaxResults</c> constant. If more results are available, they will not be included in the returned
    /// collection.</remarks>
    /// <param name="search">The criteria used to filter the audit logs. This includes optional parameters such as date range, batch ID,
    /// entity, primary key, state, and user ID.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>Task that represents the asynchronous operation. Task result contains collection of <see cref="AuditLogSearchResultModel"/> that match search criteria.</returns>
    Task<IEnumerable<AuditLogSearchResultModel>> SearchLogsAsync(AuditLogSearchModel search, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves an audit log entry by its unique identifier.
    /// </summary>
    /// <param name="id">Unique identifier of the audit log entry to retrieve.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the  <see cref="AuditLogModel"/>
    /// corresponding to the specified identifier, or <see langword="null"/>  if no matching entry is found.</returns>
    Task<AuditLogModel?> GetLogByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a collection of entity states representing the possible states of an entity.
    /// </summary>
    /// <returns>An <see cref="IEnumerable{EntityState}"/> containing the states: <see cref="EntityState.Deleted"/>, <see
    /// cref="EntityState.Added"/>, and <see cref="EntityState.Modified"/>.</returns>
    IEnumerable<EntityState> GetEntityStates();
}
