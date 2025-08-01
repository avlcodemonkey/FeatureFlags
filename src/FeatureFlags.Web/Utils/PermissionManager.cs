using System.Globalization;
using FeatureFlags.Models;
using FeatureFlags.Services;

namespace FeatureFlags.Utils;

/// <summary>
/// Provides functionality for managing permissions within the application, including registering available actions and
/// synchronizing them with the permissions database.
/// </summary>
public sealed class PermissionManager(IAssemblyService assemblyService, IPermissionService permissionService, IRoleService roleService) {
    private readonly IAssemblyService _AssemblyService = assemblyService;
    private readonly IPermissionService _PermissionService = permissionService;
    private readonly IRoleService _RoleService = roleService;

    /// <summary>
    /// Scans the assembly for all controllers and updates the permissions table to match the list of available actions.
    /// </summary>
    public async Task<bool> RegisterAsync(CancellationToken cancellationToken = default) {
        // build a list of all available actions
        var actionList = _AssemblyService.GetActionList();

        // query all permissions from db
        var permissions = (await _PermissionService.GetAllPermissionsAsync(cancellationToken))
            .ToDictionary(x => $"{x.ControllerName?.Trim()}.{x.ActionName?.Trim()}".ToLower(CultureInfo.InvariantCulture), x => x);

        // save any actions not in db
        var missingActionList = actionList.Where(x => !permissions.ContainsKey(x.Key));
        foreach (var permission in missingActionList) {
            var parts = permission.Value.Split('.');
            if (!await _PermissionService.SavePermissionAsync(new PermissionModel { ControllerName = parts[0], ActionName = parts[1] }, cancellationToken)) {
                return false;
            }
        }

        // delete any permission not in action list
        var removedPermissions = permissions.Where(x => !actionList.ContainsKey(x.Key));
        foreach (var permission in removedPermissions) {
            if (!await _PermissionService.DeletePermissionAsync(permission.Value.Id, cancellationToken)) {
                return false;
            }
        }

        // if there are no permissions in the db, then set the default role with all permissions now that we've added them
        if (permissions.Count == 0) {
            var permissionIds = (await _PermissionService.GetAllPermissionsAsync(cancellationToken)).Select(x => x.Id);
            if (!await _RoleService.AddPermissionsToDefaultRoleAsync(permissionIds, cancellationToken)) {
                return false;
            }
        }

        return true;
    }
}
