using FeatureFlags.Domain.Models;
using FeatureFlags.Models;

namespace FeatureFlags.Extensions.Services;

public static class RoleServiceExtensions {
    public static IQueryable<RoleModel> SelectAsModel(this IQueryable<Role> query)
        => query.Select(x => new RoleModel {
            Id = x.Id, Name = x.Name, IsDefault = x.IsDefault, PermissionIds = x.RolePermissions.Select(x => x.PermissionId),
            UpdatedDate = x.UpdatedDate
        });
}
