using FeatureFlags.Domain.Models;
using FeatureFlags.Models;

namespace FeatureFlags.Extensions.Services;

public static class PermissionServiceExtensions {
    public static IQueryable<PermissionModel> SelectAsModel(this IQueryable<Permission> query)
        => query.Select(x => new PermissionModel { Id = x.Id, ControllerName = x.ControllerName, ActionName = x.ActionName });
}
