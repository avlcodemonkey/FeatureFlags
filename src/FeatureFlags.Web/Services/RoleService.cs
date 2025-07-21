using System.Data;
using FeatureFlags.Domain;
using FeatureFlags.Domain.Models;
using FeatureFlags.Extensions.Services;
using FeatureFlags.Models;
using FeatureFlags.Resources;
using Microsoft.EntityFrameworkCore;

namespace FeatureFlags.Services;

public sealed class RoleService(FeatureFlagsDbContext dbContext) : IRoleService {
    private readonly FeatureFlagsDbContext _DbContext = dbContext;

    public async Task<(bool success, string message)> CopyRoleAsync(CopyRoleModel copyRoleModel, CancellationToken cancellationToken = default) {
        var role = await _DbContext.Roles.Include(x => x.RolePermissions).FirstOrDefaultAsync(x => x.Id == copyRoleModel.Id, cancellationToken);
        if (role == null) {
            return (false, Core.ErrorInvalidId);
        }

        // check that name is unique
        var nameRole = await _DbContext.Roles.FirstOrDefaultAsync(x => x.Name.ToLower() == copyRoleModel.Prompt.ToLower(), cancellationToken);
        if (nameRole != null) {
            return (false, Roles.ErrorDuplicateName);
        }

        _DbContext.Roles.Add(new Role {
            Name = copyRoleModel.Prompt, IsDefault = false,
            RolePermissions = role.RolePermissions.Select(x => new RolePermission { PermissionId = x.PermissionId }).ToList()
        });

        return await _DbContext.SaveChangesAsync(cancellationToken) > 0 ? (true, Roles.SuccessCopyingRole) : (false, Core.ErrorGeneric);
    }

    public async Task<bool> DeleteRoleAsync(int id, CancellationToken cancellationToken = default) {
        // load role with rolePermissions so auditLog tracks them being deleted
        var role = await _DbContext.Roles.Include(x => x.RolePermissions).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (role == null) {
            return false;
        }

        _DbContext.Roles.Remove(role);

        return await _DbContext.SaveChangesAsync(cancellationToken) > 0;
    }

    public async Task<IEnumerable<RoleModel>> GetAllRolesAsync(CancellationToken cancellationToken = default)
        => await _DbContext.Roles.SelectAsModel().ToListAsync(cancellationToken);

    public async Task<RoleModel?> GetDefaultRoleAsync(CancellationToken cancellationToken = default)
        => await _DbContext.Roles.Where(x => x.IsDefault).SelectAsModel().FirstOrDefaultAsync(cancellationToken);

    public async Task<RoleModel?> GetRoleByNameAsync(string name, CancellationToken cancellationToken = default)
        => await _DbContext.Roles.Where(x => x.Name.ToLower() == name.ToLower()).SelectAsModel().FirstOrDefaultAsync(cancellationToken);

    public async Task<RoleModel?> GetRoleByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _DbContext.Roles.Include(x => x.RolePermissions).Where(x => x.Id == id).SelectAsModel().FirstOrDefaultAsync(cancellationToken);

    public async Task<(bool success, string message)> SaveRoleAsync(RoleModel roleModel, CancellationToken cancellationToken = default) {
        if (roleModel.Id > 0) {
            var role = await _DbContext.Roles.Include(x => x.RolePermissions).Where(x => x.Id == roleModel.Id).FirstOrDefaultAsync(cancellationToken);
            if (role == null) {
                return (false, Core.ErrorInvalidId);
            }

            // prevent concurrent changes
            if ((role.UpdatedDate - roleModel.UpdatedDate).Seconds > 0) {
                return (false, Core.ErrorConcurrency);
            }

            // check that name is unique
            if (!await IsUniqueNameAsync(roleModel, cancellationToken)) {
                return (false, Roles.ErrorDuplicateName);
            }

            // check for only one default role
            if (!await IsUniqueDefaultRoleAsync(roleModel, cancellationToken)) {
                return (false, Roles.ErrorDuplicateDefault);
            }

            await MapToEntity(roleModel, role, cancellationToken);

            _DbContext.Roles.Update(role);
        } else {
            // check that name is unique
            if (!await IsUniqueNameAsync(roleModel, cancellationToken)) {
                return (false, Roles.ErrorDuplicateName);
            }

            // check for only one default role
            if (!await IsUniqueDefaultRoleAsync(roleModel, cancellationToken)) {
                return (false, Roles.ErrorDuplicateDefault);
            }

            var role = new Role();
            await MapToEntity(roleModel, role, cancellationToken);
            _DbContext.Roles.Add(role);
        }

        return await _DbContext.SaveChangesAsync(cancellationToken) > 0 ? (true, Roles.SuccessSavingRole) : (false, Core.ErrorGeneric);
    }

    /// <summary>
    /// Validate that role has a unique name.
    /// </summary>
    private async Task<bool> IsUniqueNameAsync(RoleModel roleModel, CancellationToken cancellationToken = default) {
        var nameRole = await _DbContext.Roles.FirstOrDefaultAsync(x => x.Name.ToLower() == roleModel.Name.ToLower(), cancellationToken);
        return nameRole == null || roleModel.Id == nameRole.Id;
    }

    /// <summary>
    /// Validate that role has a unique name.
    /// </summary>
    private async Task<bool> IsUniqueDefaultRoleAsync(RoleModel roleModel, CancellationToken cancellationToken = default) {
        if (!roleModel.IsDefault) {
            return true;
        }
        // check that there is only one default role
        var defaultRole = await _DbContext.Roles.FirstOrDefaultAsync(x => x.IsDefault, cancellationToken);
        return defaultRole == null || roleModel.Id == defaultRole.Id;
    }

    public async Task<bool> AddPermissionsToDefaultRoleAsync(IEnumerable<int> permissionIds, CancellationToken cancellationToken = default) {
        var defaultRole = await _DbContext.Roles.FirstOrDefaultAsync(x => x.IsDefault, cancellationToken);
        if (defaultRole == null) {
            return false;
        }

        defaultRole.RolePermissions = permissionIds.Select(x => new RolePermission { PermissionId = x, RoleId = defaultRole.Id }).ToList();

        return await _DbContext.SaveChangesAsync(cancellationToken) > 0;
    }

    /// <summary>
    /// Maps data from the role model onto the role entity.
    /// </summary>
    private async Task MapToEntity(RoleModel roleModel, Role role, CancellationToken cancellationToken = default) {
        role.Name = roleModel.Name;
        role.IsDefault = roleModel.IsDefault;

        var existingPermissions = new Dictionary<int, RolePermission>();
        if (role.Id > 0 && roleModel.PermissionIds?.Any() == true) {
            existingPermissions = (await _DbContext.RolePermissions.Where(x => x.RoleId == role.Id).ToListAsync(cancellationToken)).ToDictionary(x => x.PermissionId, x => x);
        }

        role.RolePermissions = roleModel.PermissionIds?.Select(x => {
            if (existingPermissions.TryGetValue(x, out var rolePermission)) {
                return rolePermission;
            }
            return new RolePermission { PermissionId = x, RoleId = roleModel.Id };
        }).ToList() ?? [];
    }
}
