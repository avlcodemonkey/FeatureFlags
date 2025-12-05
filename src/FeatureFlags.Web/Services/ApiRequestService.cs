using FeatureFlags.Domain;
using FeatureFlags.Models;
using Microsoft.EntityFrameworkCore;

namespace FeatureFlags.Services;

/// <inheritdoc />
public sealed class ApiRequestService(FeatureFlagsDbContext dbContext) : IApiRequestService {
    private readonly FeatureFlagsDbContext _DbContext = dbContext;

    /// <inheritdoc />
    public async Task<IEnumerable<ApiRequestModel>> GetApiRequestsAsync(int? userId, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default) {
        var query = _DbContext.ApiRequests.AsQueryable();

        if (userId.HasValue) {
            query = query.Where(x => x.ApiKey.UserId == userId.Value);
        }

        if (startDate.HasValue) {
            query = query.Where(x => x.RequestedDate >= startDate.Value);
        }

        if (endDate.HasValue) {
            query = query.Where(x => x.RequestedDate <= endDate.Value);
        }

        return await query
            .OrderByDescending(x => x.RequestedDate)
            .Select(x => new ApiRequestModel {
                Id = x.Id,
                ApiKeyId = x.ApiKeyId,
                IpAddress = x.IpAddress,
                RequestedDate = x.RequestedDate
            })
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Dictionary<string, int>> GetApiRequestsByApiKeyAsync(int? userId, DateTime? startDate, DateTime? endDate, int? maxResults, CancellationToken cancellationToken = default) {
        var query = _DbContext.ApiRequests.AsQueryable();

        if (userId.HasValue) {
            query = query.Where(x => x.ApiKey.UserId == userId.Value);
        }
        if (startDate.HasValue) {
            query = query.Where(x => x.RequestedDate >= startDate.Value);
        }
        if (endDate.HasValue) {
            query = query.Where(x => x.RequestedDate <= endDate.Value);
        }

        var results = await query
            .GroupBy(x => x.ApiKey.Name)
            .Select(group => new { ApiKeyName = group.Key, Count = group.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync();

        if (maxResults.HasValue) {
            results = results.Take(maxResults.Value).ToList();
        }

        return results.ToDictionary(x => x.ApiKeyName, x => x.Count);
    }

    /// <inheritdoc />
    public async Task<Dictionary<string, int>> GetApiRequestsByIpAddressAsync(int? userId, DateTime? startDate, DateTime? endDate, int? maxResults, CancellationToken cancellationToken = default) {
        var query = _DbContext.ApiRequests.AsQueryable();

        if (userId.HasValue) {
            query = query.Where(x => x.ApiKey.UserId == userId.Value);
        }

        if (startDate.HasValue) {
            query = query.Where(x => x.RequestedDate >= startDate.Value);
        }

        if (endDate.HasValue) {
            query = query.Where(x => x.RequestedDate <= endDate.Value);
        }

        var results = await query
            .GroupBy(x => x.IpAddress)
            .Select(group => new { IpAddress = group.Key, Count = group.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync();

        if (maxResults.HasValue) {
            results = results.Take(maxResults.Value).ToList();
        }

        return results.ToDictionary(x => x.IpAddress, x => x.Count);

    }

    /// <inheritdoc />
    public async Task<bool> SaveApiRequestAsync(int apiKeyId, string ipAddress, CancellationToken cancellationToken = default) {
        ArgumentNullException.ThrowIfNull(ipAddress);

        var apiRequest = new Domain.Models.ApiRequest {
            ApiKeyId = apiKeyId,
            IpAddress = ipAddress
        };

        _DbContext.ApiRequests.Add(apiRequest);
        return await _DbContext.SaveChangesAsync(cancellationToken) > 0;
    }
}
