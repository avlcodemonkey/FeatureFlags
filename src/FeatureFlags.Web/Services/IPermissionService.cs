using FeatureFlags.Models;

namespace FeatureFlags.Services;

/// <summary>
/// Provides permission management operations for the application.
/// </summary>
public interface IPermissionService {
    /// <summary>
    /// Deletes a permission by unique identifier.
    /// </summary>
    /// <param name="permissionId">Unique identifier of permission to delete.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>True if permission was deleted; otherwise, false.</returns>
    Task<bool> DeletePermissionAsync(int permissionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all permissions in the system.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>Collection of permission models.</returns>
    Task<IEnumerable<PermissionModel>> GetAllPermissionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves a permission to the system.
    /// </summary>
    /// <param name="permissionModel">Permission model to save.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>True if permission was saved; otherwise, false.</returns>
    Task<bool> SavePermissionAsync(PermissionModel permissionModel, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves permissions grouped by controller.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>Dictionary mapping controller names to lists of permission models.</returns>
    Task<Dictionary<string, List<PermissionModel>>> GetControllerPermissionsAsync(CancellationToken cancellationToken = default);
}
