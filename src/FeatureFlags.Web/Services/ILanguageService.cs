using FeatureFlags.Models;

namespace FeatureFlags.Services;

/// <summary>
/// Fetches languages from the DB.
/// Use AsNoTracking since languages can't be modified.
/// </summary>
public interface ILanguageService {
    Task<IEnumerable<LanguageModel>> GetAllLanguagesAsync(CancellationToken cancellationToken = default);

    Task<LanguageModel?> GetLanguageByIdAsync(int id, CancellationToken cancellationToken = default);
}
