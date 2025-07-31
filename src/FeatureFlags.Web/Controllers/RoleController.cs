using FeatureFlags.Attributes;
using FeatureFlags.Extensions;
using FeatureFlags.Models;
using FeatureFlags.Resources;
using FeatureFlags.Services;
using FeatureFlags.Utils;
using Microsoft.AspNetCore.Mvc;

namespace FeatureFlags.Controllers;

/// <summary>
/// Provides functionality for managing roles and permissions within the application.
/// </summary>
public class RoleController(IRoleService roleService, IPermissionService permissionService, IAssemblyService assemblyService, ILogger<RoleController> logger)
    : BaseController(logger) {

    private readonly IPermissionService _PermissionService = permissionService;
    private readonly IRoleService _RoleService = roleService;
    private readonly IAssemblyService _AssemblyService = assemblyService;

    private const string _IndexView = "Index";
    private const string _CreateEditView = "CreateEdit";
    private const string _CopyView = "Copy";

    /// <summary>
    /// Renders the role landing page with the table of roles.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken = default) {
        var defaultRole = await _RoleService.GetDefaultRoleAsync(cancellationToken);
        if (defaultRole == null) {
            return ViewWithError(_IndexView, null, Roles.ErrorNoDefaultRole);
        }
        return View(_IndexView);
    }

    /// <summary>
    /// Returns the role list as json.
    /// </summary>
    [HttpGet, ParentAction(nameof(Index)), AjaxRequestOnly]
    public async Task<IActionResult> List(CancellationToken cancellationToken = default)
        => Ok((await _RoleService.GetAllRolesAsync(cancellationToken)).Select(x => new RoleListResultModel { Id = x.Id, Name = x.Name }));

    /// <summary>
    /// Renders the form to create a role.
    /// </summary>
    [HttpGet, ParentAction(nameof(Edit))]
    public IActionResult Create() => View(_CreateEditView, new RoleModel());

    /// <summary>
    /// Saves new role if valid. Renders the create page on error, or redirects to index.
    /// </summary>
    [HttpPost, ParentAction(nameof(Edit)), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RoleModel model, CancellationToken cancellationToken = default) => await Save(model, cancellationToken);

    /// <summary>
    /// Renders the form to edit a role, or index on error.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken = default) {
        var model = await _RoleService.GetRoleByIdAsync(id, cancellationToken);
        if (model == null) {
            ViewData.AddError(Core.ErrorInvalidId);
            return await Index(cancellationToken);
        }
        return View(_CreateEditView, model);
    }

    /// <summary>
    /// Saves updated role if valid. Renders the edit page on error, or redirects to index.
    /// </summary>
    [HttpPut, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(RoleModel model, CancellationToken cancellationToken = default)
        => await Save(model, cancellationToken);

    /// <summary>
    /// Renders the form to copy a role, or index page on error.
    /// </summary>
    [HttpGet, ParentAction(nameof(Edit))]
    public async Task<IActionResult> Copy(int id, CancellationToken cancellationToken = default) {
        var model = await _RoleService.GetRoleByIdAsync(id, cancellationToken);
        if (model == null) {
            ViewData.AddError(Core.ErrorInvalidId);
            return await Index(cancellationToken);
        }

        return View(_CopyView, new CopyRoleModel { Id = model.Id, Prompt = Core.CopyOf.Replace("{0}", model.Name) });
    }

    /// <summary>
    /// Copies the role if valid. Renders the copy page on error, or redirects to index.
    /// </summary>
    [HttpPost, ParentAction(nameof(Edit))]
    public async Task<IActionResult> Copy(CopyRoleModel model, CancellationToken cancellationToken = default) {
        if (!ModelState.IsValid) {
            return ViewWithError(_CopyView, model, ModelState);
        }

        (var success, var message) = await _RoleService.CopyRoleAsync(model, cancellationToken);
        if (!success) {
            return ViewWithError(_CopyView, model, message);
        }

        ViewData.AddMessage(Roles.SuccessCopyingRole);
        return await IndexWithPushState(cancellationToken);
    }

    /// <summary>
    /// Deletes the role if valid, and renders the index page.
    /// </summary>
    [HttpDelete]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default) {
        if (!await _RoleService.DeleteRoleAsync(id, cancellationToken)) {
            ViewData.AddError(Roles.ErrorDeletingRole);
            return await IndexWithPushState(cancellationToken);
        }

        ViewData.AddMessage(Roles.SuccessDeletingRole);
        return await IndexWithPushState(cancellationToken);
    }

    /// <summary>
    /// Generates a list of all permissions and sync's the database. Renders the index page.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> RefreshPermissions(CancellationToken cancellationToken = default) {
        if (!await new PermissionManager(_AssemblyService, _PermissionService, _RoleService).RegisterAsync(cancellationToken)) {
            ViewData.AddError(Roles.ErrorRefreshingPermissions);
            return await IndexWithPushState(cancellationToken);
        }

        ViewData.AddMessage(Roles.SuccessRefreshingPermissions);
        return await IndexWithPushState(cancellationToken);
    }

    /// <summary>
    /// Save the model using the role service.  Used by Create and Edit.
    /// </summary>
    private async Task<IActionResult> Save(RoleModel model, CancellationToken cancellationToken = default) {
        if (!ModelState.IsValid) {
            return ViewWithError(_CreateEditView, model, ModelState);
        }

        (var success, var message) = await _RoleService.SaveRoleAsync(model, cancellationToken);
        if (!success) {
            return ViewWithError(_CreateEditView, model, message);
        }

        ViewData.AddMessage(Roles.SuccessSavingRole);
        return await IndexWithPushState(cancellationToken);
    }

    /// <summary>
    /// Helper method to add a header to pushState to Index and return the Index action.
    /// </summary>
    private async Task<IActionResult> IndexWithPushState(CancellationToken cancellationToken = default) {
        AddPushState(nameof(Index));
        return await Index(cancellationToken);
    }
}
