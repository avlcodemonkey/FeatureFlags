using FeatureFlags.Models;

namespace FeatureFlags.Services;

/// <summary>
/// Provides role management operations for the application.
/// </summary>
public interface IRoleService {
    /// <summary>
    /// Copies a role using the provided model.
    /// </summary>
    /// <param name="copyRoleModel">Model containing data for copying a role.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>Tuple indicating success and message.</returns>
    Task<(bool success, string message)> CopyRoleAsync(CopyRoleModel copyRoleModel, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a role by unique identifier.
    /// </summary>
    /// <param name="id">Unique identifier of role to delete.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>True if role was deleted; otherwise, false.</returns>
    Task<bool> DeleteRoleAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all roles in the system.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>Collection of role models.</returns>
    Task<IEnumerable<RoleModel>> GetAllRolesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the default role.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>Default role model if found; otherwise, null.</returns>
    Task<RoleModel?> GetDefaultRoleAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a role by name.
    /// </summary>
    /// <param name="name">Name of role.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>Role model if found; otherwise, null.</returns>
    Task<RoleModel?> GetRoleByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a role by unique identifier.
    /// </summary>
    /// <param name="id">Unique identifier of role.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>Role model if found; otherwise, null.</returns>
    Task<RoleModel?> GetRoleByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves a role to the system.
    /// </summary>
    /// <param name="roleModel">Role model to save.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>Tuple indicating success and message.</returns>
    Task<(bool success, string message)> SaveRoleAsync(RoleModel roleModel, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds permissions to the default role.
    /// </summary>
    /// <param name="permissionIds">Collection of permission IDs to add.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>True if permissions were added; otherwise, false.</returns>
    Task<bool> AddPermissionsToDefaultRoleAsync(IEnumerable<int> permissionIds, CancellationToken cancellationToken = default);
}
