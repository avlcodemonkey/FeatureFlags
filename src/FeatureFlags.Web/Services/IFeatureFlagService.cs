using FeatureFlags.Models;

namespace FeatureFlags.Services;

public interface IFeatureFlagService {
    Task<IEnumerable<FeatureFlagModel>> GetAllFeatureFlagsAsync(CancellationToken cancellationToken = default);

    Task<IEnumerable<FeatureFlagModel>> GetCachedFeatureFlagsAsync(CancellationToken cancellationToken = default);

    Task<FeatureFlagModel?> GetFeatureFlagByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<(bool success, string message)> SaveFeatureFlagAsync(FeatureFlagModel featureFlagModel, CancellationToken cancellationToken = default);

    Task<bool> DeleteFeatureFlagAsync(int id, CancellationToken cancellationToken = default);

    void ClearCache();
}
