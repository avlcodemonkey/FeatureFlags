using FeatureFlags.Models;

namespace FeatureFlags.Services;

public interface IPermissionService {
    Task<bool> DeletePermissionAsync(int permissionId, CancellationToken cancellationToken = default);

    Task<IEnumerable<PermissionModel>> GetAllPermissionsAsync(CancellationToken cancellationToken = default);

    Task<bool> SavePermissionAsync(PermissionModel permissionModel, CancellationToken cancellationToken = default);

    Task<Dictionary<string, List<PermissionModel>>> GetControllerPermissionsAsync(CancellationToken cancellationToken = default);
}
