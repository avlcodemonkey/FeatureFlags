using FeatureFlags.Domain.Models;
using FeatureFlags.Models;

namespace FeatureFlags.Extensions.Services;

/// <summary>
/// Helpers methods for API key service.
/// </summary>
public static class ApiKeyServiceExtensions {
    /// <summary>
    /// Converts entity to model.
    /// </summary>
    /// <param name="query">Source sequence of <see cref="ApiKey"/> objects to project.</param>
    /// <returns><see cref="IQueryable{ApiKeyModel}"/> sequence where each element is a projection of the corresponding element in the source sequence.</returns>
    public static IQueryable<ApiKeyModel> SelectAsModel(this IQueryable<ApiKey> query)
        => query.Select(x => new ApiKeyModel { Id = x.Id, Name = x.Name, Key = new string('x', 32), CreatedDate = x.CreatedDate, UpdatedDate = x.UpdatedDate });
}
