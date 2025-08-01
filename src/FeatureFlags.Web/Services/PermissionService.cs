using FeatureFlags.Domain;
using FeatureFlags.Domain.Models;
using FeatureFlags.Extensions.Services;
using FeatureFlags.Models;
using Microsoft.EntityFrameworkCore;

namespace FeatureFlags.Services;

/// <inheritdoc />
public sealed class PermissionService(FeatureFlagsDbContext dbContext) : IPermissionService {
    private readonly FeatureFlagsDbContext _DbContext = dbContext;

    /// <inheritdoc />
    public async Task<bool> DeletePermissionAsync(int permissionId, CancellationToken cancellationToken = default) {
        var permission = await _DbContext.Permissions.FirstOrDefaultAsync(x => x.Id == permissionId, cancellationToken);
        if (permission == null) {
            return false;
        }

        _DbContext.Remove(permission);
        return await _DbContext.SaveChangesAsync(cancellationToken) > 0;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<PermissionModel>> GetAllPermissionsAsync(CancellationToken cancellationToken = default)
        => await _DbContext.Permissions.SelectAsModel().ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<bool> SavePermissionAsync(PermissionModel permissionModel, CancellationToken cancellationToken = default) {
        if (permissionModel.Id > 0) {
            var permission = await _DbContext.Permissions.Where(x => x.Id == permissionModel.Id).FirstOrDefaultAsync(cancellationToken);
            if (permission == null) {
                return false;
            }

            MapToEntity(permissionModel, permission);
            _DbContext.Permissions.Update(permission);
        } else {
            var permission = new Permission();
            MapToEntity(permissionModel, permission);
            _DbContext.Permissions.Add(permission);
        }

        return await _DbContext.SaveChangesAsync(cancellationToken) > 0;
    }

    /// <inheritdoc />
    public async Task<Dictionary<string, List<PermissionModel>>> GetControllerPermissionsAsync(CancellationToken cancellationToken = default) {
        var controllerPermissions = new Dictionary<string, List<PermissionModel>>();
        var permissions = await GetAllPermissionsAsync(cancellationToken);

        foreach (var permission in permissions) {
            if (!controllerPermissions.TryGetValue(permission.ControllerName, out _)) {
                controllerPermissions.Add(permission.ControllerName, []);
            }
            controllerPermissions[permission.ControllerName].Add(permission);
        }

        return controllerPermissions;
    }

    /// <summary>
    /// Maps data from the permission model onto the permission entity.
    /// </summary>
    private static void MapToEntity(PermissionModel permissionModel, Permission permission) {
        permission.Id = permissionModel.Id;
        permission.ControllerName = permissionModel.ControllerName;
        permission.ActionName = permissionModel.ActionName;
    }
}
