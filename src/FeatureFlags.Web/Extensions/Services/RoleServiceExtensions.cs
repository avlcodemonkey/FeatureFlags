using FeatureFlags.Domain.Models;
using FeatureFlags.Models;

namespace FeatureFlags.Extensions.Services;

/// <summary>
/// Provides extension methods for querying and transforming role data.
/// </summary>
public static class RoleServiceExtensions {
    /// <summary>
    /// Projects a queryable sequence of <see cref="Role"/> entities into a queryable sequence of <see cref="RoleModel"/> objects.
    /// </summary>
    /// <param name="query">Queryable sequence of <see cref="Role"/> entities to project.</param>
    /// <returns>Queryable sequence of <see cref="RoleModel"/> objects.</returns>
    public static IQueryable<RoleModel> SelectAsModel(this IQueryable<Role> query)
        => query.Select(x => new RoleModel {
            Id = x.Id, Name = x.Name, IsDefault = x.IsDefault, PermissionIds = x.RolePermissions.Select(x => x.PermissionId),
            UpdatedDate = x.UpdatedDate
        });
}
