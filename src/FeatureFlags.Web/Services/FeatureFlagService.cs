using System.Data;
using FeatureFlags.Constants;
using FeatureFlags.Domain;
using FeatureFlags.Domain.Models;
using FeatureFlags.Extensions.Services;
using FeatureFlags.Models;
using FeatureFlags.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FeatureFlags.Services;

public sealed class FeatureFlagService(FeatureFlagsDbContext dbContext, IMemoryCache memoryCache) : IFeatureFlagService {
    private readonly FeatureFlagsDbContext _DbContext = dbContext;
    private readonly IMemoryCache _MemoryCache = memoryCache;

    private const string _CacheKey = "FeatureFlags";

    public async Task<IEnumerable<FeatureFlagModel>> GetAllFeatureFlagsAsync(CancellationToken cancellationToken = default)
        => await _DbContext.FeatureFlags.SelectAsModel().ToListAsync(cancellationToken);

    /// <summary>
    /// Special endpoint for the feature definition provider to use.
    /// </summary>
    public async Task<IEnumerable<FeatureFlagModel>> GetCachedFeatureFlagsAsync(CancellationToken cancellationToken = default)
        => await _MemoryCache.GetOrCreateAsync(_CacheKey, async (x) => {
            x.SetAbsoluteExpiration(TimeSpan.FromMinutes(FeatureFlagConstants.CacheLifeTime));
            return await GetAllFeatureFlagsAsync(cancellationToken);
        }) ?? new List<FeatureFlagModel>();

    public async Task<(bool success, string message)> SaveFeatureFlagAsync(FeatureFlagModel featureFlagModel, CancellationToken cancellationToken = default) {
        if (featureFlagModel.Id > 0) {
            var featureFlag = await _DbContext.FeatureFlags.Where(x => x.Id == featureFlagModel.Id).FirstOrDefaultAsync(cancellationToken);
            if (featureFlag == null) {
                return (false, Core.ErrorInvalidId);
            }

            // prevent concurrent changes
            if (featureFlag.UpdatedDate > featureFlagModel.UpdatedDate) {
                return (false, Core.ErrorConcurrency);
            }

            // check that name is unique
            if (!await IsUniqueNameAsync(featureFlagModel, cancellationToken)) {
                return (false, Flags.ErrorDuplicateName);
            }

            featureFlag.Name = featureFlagModel.Name;
            featureFlag.IsEnabled = featureFlagModel.IsEnabled;

            _DbContext.FeatureFlags.Update(featureFlag);
        } else {
            // check that name is unique
            if (!await IsUniqueNameAsync(featureFlagModel, cancellationToken)) {
                return (false, Flags.ErrorDuplicateName);
            }

            var featureFlag = new FeatureFlag {
                Name = featureFlagModel.Name,
                IsEnabled = featureFlagModel.IsEnabled
            };
            _DbContext.FeatureFlags.Add(featureFlag);
        }

        if (await _DbContext.SaveChangesAsync(cancellationToken) > 0) {
            ClearCache();
            return (true, Flags.SuccessSavingFlag);
        }
        return (false, Core.ErrorGeneric);
    }

    /// <summary>
    /// Validate that feature flag has a unique name.
    /// </summary>
    private async Task<bool> IsUniqueNameAsync(FeatureFlagModel featureFlagModel, CancellationToken cancellationToken = default) {
        var namedFlag = await _DbContext.FeatureFlags.FirstOrDefaultAsync(x => x.Name == featureFlagModel.Name, cancellationToken);
        return namedFlag == null || featureFlagModel.Id == namedFlag.Id;
    }

    /// <summary>
    /// Find a feature flag by id.
    /// </summary>
    /// <returns></returns>
    public async Task<FeatureFlagModel?> GetFeatureFlagByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _DbContext.FeatureFlags.Where(x => x.Id == id).SelectAsModel().FirstOrDefaultAsync(cancellationToken);

    /// <summary>
    /// Delete a feature flag by id.
    /// </summary>
    public async Task<bool> DeleteFeatureFlagAsync(int id, CancellationToken cancellationToken = default) {
        // load feature flag so auditLog tracks it being deleted
        var feature = await _DbContext.FeatureFlags.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (feature == null) {
            return false;
        }

        _DbContext.FeatureFlags.Remove(feature);

        if (await _DbContext.SaveChangesAsync(cancellationToken) > 0) {
            ClearCache();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Clears the cache of feature flags.
    /// </summary>
    public void ClearCache() => _MemoryCache.Remove(_CacheKey);
}
