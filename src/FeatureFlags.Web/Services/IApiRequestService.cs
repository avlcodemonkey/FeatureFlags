using FeatureFlags.Models;

namespace FeatureFlags.Services;

/// <summary>
/// Provides methods for tracking API requests.
/// </summary>
public interface IApiRequestService {
    /// <summary>
    /// Retrieves all API requests matching criteria.
    /// </summary>
    /// <param name="userId">The user ID to filter API requests. If null, no user ID filter is applied.</param>
    /// <param name="startDate">The start date to filter API requests. If null, no start date filter is applied.</param>
    /// <param name="endDate">The end date to filter API requests. If null, no end date filter is applied.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/> that can be used to cancel the operation. Default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>Task that represents the asynchronous operation. Task result contains an enumerable collection of <see cref="ApiRequestModel"/> objects.</returns>
    Task<IEnumerable<ApiRequestModel>> GetApiRequestsAsync(int? userId, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves total of API requests matching criteria group by api key.
    /// </summary>
    /// <param name="userId">The user ID to filter API requests. If null, no user ID filter is applied.</param>
    /// <param name="startDate">The start date to filter API requests. If null, no start date filter is applied.</param>
    /// <param name="endDate">The end date to filter API requests. If null, no end date filter is applied.</param>
    /// <param name="maxResults">The maximum number of results to return. If null, all results are returned.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/> that can be used to cancel the operation. Default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>Task that represents the asynchronous operation. Task result contains a dictionary of key names and request count.</returns>
    Task<Dictionary<string, int>> GetApiRequestsByApiKeyAsync(int? userId, DateTime? startDate, DateTime? endDate, int? maxResults, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves total of API requests matching criteria group by ip address.
    /// </summary>
    /// <param name="userId">The user ID to filter API requests. If null, no user ID filter is applied.</param>
    /// <param name="startDate">The start date to filter API requests. If null, no start date filter is applied.</param>
    /// <param name="endDate">The end date to filter API requests. If null, no end date filter is applied.</param>
    /// <param name="maxResults">The maximum number of results to return. If null, all results are returned.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/> that can be used to cancel the operation. Default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>Task that represents the asynchronous operation. Task result contains a dictionary of addresses and request count.</returns>
    Task<Dictionary<string, int>> GetApiRequestsByIpAddressAsync(int? userId, DateTime? startDate, DateTime? endDate, int? maxResults, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves the specified API request.
    /// </summary>
    /// <param name="apiKeyId">The ID of the API key associated with the request.</param>
    /// <param name="ipAddress">The IP address from which the API request originated.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/> that can be used to cancel the operation. Default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>Boolean that indicates whether the operation was successful.</returns>
    Task<bool> SaveApiRequestAsync(int apiKeyId, string ipAddress, CancellationToken cancellationToken = default);
}
