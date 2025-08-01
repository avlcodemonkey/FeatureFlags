using FeatureFlags.Models;

namespace FeatureFlags.Services;

/// <summary>
/// Fetches languages from the database.
/// </summary>
public interface ILanguageService {
    /// <summary>
    /// Retrieves all languages.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>Collection of language models.</returns>
    Task<IEnumerable<LanguageModel>> GetAllLanguagesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a language by unique identifier.
    /// </summary>
    /// <param name="id">Unique identifier of language.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>Language model if found; otherwise, null.</returns>
    Task<LanguageModel?> GetLanguageByIdAsync(int id, CancellationToken cancellationToken = default);
}
