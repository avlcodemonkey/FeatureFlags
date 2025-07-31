using FeatureFlags.Models;

namespace FeatureFlags.Services;

/// <summary>
/// Provides methods for managing API keys, including retrieving, saving, and deleting API keys.
/// </summary>
public interface IApiKeyService {
    /// <summary>
    /// Retrieves all API keys available in the system.
    /// </summary>
    /// <param name="cancellationToken"><see cref="CancellationToken"/> that can be used to cancel the operation. Default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>Task that represents the asynchronous operation. Task result contains an enumerable collection of <see cref="ApiKeyModel"/> objects.</returns>
    Task<IEnumerable<ApiKeyModel>> GetAllApiKeysAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves an API key by its key.
    /// </summary>
    /// <param name="key">The key of the API key to retrieve. This value cannot be null or empty.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/> that can be used to cancel the operation. Default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A <see cref="ApiKeyModel"/> representing the API key if found; otherwise, <see langword="null"/>.</returns>
    Task<ApiKeyModel?> GetApiKeyByKeyAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves the specified API key.
    /// </summary>
    /// <param name="apiKeyModel">The API key model to be saved. This parameter cannot be null.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/> that can be used to cancel the operation. Default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>Tuple containing a boolean that indicates whether the operation was successful, and a string with additional details about the result.</returns>
    Task<(bool success, string message)> SaveApiKeyAsync(ApiKeyModel apiKeyModel, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an API key with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the API key to delete. Must be a positive integer.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/> that can be used to cancel the operation. Default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns><see langword="true"/> if the API key was successfully deleted; otherwise, <see langword="false"/>.</returns>
    Task<bool> DeleteApiKeyAsync(int id, CancellationToken cancellationToken = default);
}
