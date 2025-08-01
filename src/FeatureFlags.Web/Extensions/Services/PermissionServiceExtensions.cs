using FeatureFlags.Domain.Models;
using FeatureFlags.Models;

namespace FeatureFlags.Extensions.Services;

/// <summary>
/// Provides extension methods for querying and transforming <see cref="Permission"/> entities.
/// </summary>
public static class PermissionServiceExtensions {
    /// <summary>
    /// Projects a queryable sequence of <see cref="Permission"/> entities into a sequence of <see cref="PermissionModel"/> objects.
    /// </summary>
    /// <param name="query">Queryable sequence of <see cref="Permission"/> entities to project.</param>
    /// <returns><see cref="IQueryable{T}"/> containing <see cref="PermissionModel"/> objects.</returns>
    public static IQueryable<PermissionModel> SelectAsModel(this IQueryable<Permission> query)
        => query.Select(x => new PermissionModel { Id = x.Id, ControllerName = x.ControllerName, ActionName = x.ActionName });
}
