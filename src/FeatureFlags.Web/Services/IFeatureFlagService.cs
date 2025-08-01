using FeatureFlags.Models;

namespace FeatureFlags.Services;

/// <summary>
/// Provides methods for managing feature flags, including retrieving, saving, and deleting feature flags.
/// </summary>
public interface IFeatureFlagService {
    /// <summary>
    /// Retrieves all feature flags available in the system.
    /// </summary>
    /// <param name="cancellationToken"><see cref="CancellationToken"/> that can be used to cancel the operation. Default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>Task that represents the asynchronous operation. Task result contains an enumerable collection of <see cref="FeatureFlagModel"/> objects.</returns>
    Task<IEnumerable<FeatureFlagModel>> GetAllFeatureFlagsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a feature flag by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the feature flag to retrieve. Must be a positive integer.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/> that can be used to cancel the operation. Default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>Task that represents the asynchronous operation. Task result contains the flag as a <see cref="FeatureFlagModel"/> if found else <see langword="null"/>.</returns>
    Task<FeatureFlagModel?> GetFeatureFlagByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a feature flag by its name asynchronously.
    /// </summary>
    /// <param name="name">The name of the feature flag to retrieve. This value cannot be null or empty.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/> that can be used to cancel the operation. Default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A <see cref="FeatureFlagModel"/> representing the feature flag if found; otherwise, <see langword="null"/>.</returns>
    Task<FeatureFlagModel?> GetFeatureFlagByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves the specified feature flag asynchronously.
    /// </summary>
    /// <param name="featureFlagModel">The feature flag model to be saved. This parameter cannot be null.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/> that can be used to cancel the operation. Default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>Tuple containing a boolean that indicates whether the operation was successful, and a string with additional details about the result.</returns>
    Task<(bool success, string message)> SaveFeatureFlagAsync(FeatureFlagModel featureFlagModel, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a feature flag with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the feature flag to delete. Must be a positive integer.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/> that can be used to cancel the operation. Default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns><see langword="true"/> if the feature flag was successfully deleted; otherwise, <see langword="false"/>.</returns>
    Task<bool> DeleteFeatureFlagAsync(int id, CancellationToken cancellationToken = default);
}
