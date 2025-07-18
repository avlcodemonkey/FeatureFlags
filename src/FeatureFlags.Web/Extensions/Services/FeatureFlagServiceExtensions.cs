using FeatureFlags.Domain.Models;
using FeatureFlags.Models;

namespace FeatureFlags.Extensions.Services;

public static class FeatureFlagServiceExtensions {
    public static IQueryable<FeatureFlagModel> SelectAsModel(this IQueryable<FeatureFlag> query)
        => query.Select(x => new FeatureFlagModel { Id = x.Id, Name = x.Name, IsEnabled = x.IsEnabled, UpdatedDate = x.UpdatedDate });
}
