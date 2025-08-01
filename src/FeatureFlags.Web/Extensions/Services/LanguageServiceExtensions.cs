using FeatureFlags.Domain.Models;
using FeatureFlags.Models;

namespace FeatureFlags.Extensions.Services;

/// <summary>
/// Provides extension methods for querying language-related data.
/// </summary>
public static class LanguageServiceExtensions {
    /// <summary>
    /// Projects a sequence of <see cref="Language"/> entities into a sequence of <see cref="LanguageModel"/> objects.
    /// </summary>
    /// <param name="query">Source queryable sequence of <see cref="Language"/> entities.</param>
    /// <returns><see cref="IQueryable{T}"/> containing <see cref="LanguageModel"/> objects.</returns>
    public static IQueryable<LanguageModel> SelectAsModel(this IQueryable<Language> query)
        => query.Select(x => new LanguageModel { Id = x.Id, Name = x.Name, IsDefault = x.IsDefault, LanguageCode = x.LanguageCode });
}
