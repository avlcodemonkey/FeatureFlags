using FeatureFlags.Models;

namespace FeatureFlags.Services;

public interface IRoleService {
    Task<(bool success, string message)> CopyRoleAsync(CopyRoleModel copyRoleModel, CancellationToken cancellationToken = default);

    Task<bool> DeleteRoleAsync(int id, CancellationToken cancellationToken = default);

    Task<IEnumerable<RoleModel>> GetAllRolesAsync(CancellationToken cancellationToken = default);

    Task<RoleModel?> GetDefaultRoleAsync(CancellationToken cancellationToken = default);

    Task<RoleModel?> GetRoleByNameAsync(string name, CancellationToken cancellationToken = default);

    Task<RoleModel?> GetRoleByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<(bool success, string message)> SaveRoleAsync(RoleModel roleModel, CancellationToken cancellationToken = default);

    Task<bool> AddPermissionsToDefaultRoleAsync(IEnumerable<int> permissionIds, CancellationToken cancellationToken = default);
}
