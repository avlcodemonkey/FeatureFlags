using FeatureFlags.Domain.Models;
using FeatureFlags.Models;

namespace FeatureFlags.Extensions.Services;

/// <summary>
/// Provides extension methods for querying feature flags.
/// </summary>
public static class FeatureFlagServiceExtensions {
    /// <summary>
    /// Projects a queryable collection of <see cref="FeatureFlag"/> entities into a collection of <see cref="FeatureFlagModel"/> objects.
    /// </summary>
    /// <param name="query">Queryable collection of <see cref="FeatureFlag"/> entities to project.</param>
    /// <returns><see cref="IQueryable{T}"/> containing <see cref="FeatureFlagModel"/> objects.</returns>
    public static IQueryable<FeatureFlagModel> SelectAsModel(this IQueryable<FeatureFlag> query)
        => query.Select(x => new FeatureFlagModel {
            Id = x.Id, Name = x.Name, Status = x.Status,
            RequirementType = (Constants.RequirementType)x.RequirementType, UpdatedDate = x.UpdatedDate
        });
}
