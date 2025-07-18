using FeatureFlags.Domain.Models;
using FeatureFlags.Models;

namespace FeatureFlags.Extensions.Services;

public static class LanguageServiceExtensions {
    public static IQueryable<LanguageModel> SelectAsModel(this IQueryable<Language> query)
        => query.Select(x => new LanguageModel { Id = x.Id, Name = x.Name, IsDefault = x.IsDefault, LanguageCode = x.LanguageCode });
}
