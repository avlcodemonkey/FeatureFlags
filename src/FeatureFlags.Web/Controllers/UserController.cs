using FeatureFlags.Attributes;
using FeatureFlags.Extensions;
using FeatureFlags.Models;
using FeatureFlags.Resources;
using FeatureFlags.Services;
using Microsoft.AspNetCore.Mvc;

namespace FeatureFlags.Controllers;

public class UserController(IUserService userService, ILogger<UserController> logger) : BaseController(logger) {
    private readonly IUserService _UserService = userService;

    private const string _IndexView = "Index";
    private const string _CreateEditView = "CreateEdit";

    /// <summary>
    /// Renders the user landing page with the table of users.
    /// </summary>
    [HttpGet]
    public IActionResult Index() => View(_IndexView);

    /// <summary>
    /// Returns the user list as json.
    /// </summary>
    [HttpGet, ParentAction(nameof(Index)), AjaxRequestOnly]
    public async Task<IActionResult> List(CancellationToken cancellationToken = default)
        => Ok((await _UserService.GetAllUsersAsync(cancellationToken)).Select(x =>
            new UserListResultModel { Id = x.Id, Name = x.Name, Email = x.Email }
        ));

    /// <summary>
    /// Renders the form to create a user.
    /// </summary>
    [HttpGet]
    public IActionResult Create() => View(_CreateEditView, new UserModel());

    /// <summary>
    /// Saves new user if valid. Renders the create page on error, or redirects to index page.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserModel model, CancellationToken cancellationToken = default) => await Save(model, cancellationToken);

    /// <summary>
    /// Renders the form to edit a user, or index page on error.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken = default) {
        var model = await _UserService.GetUserByIdAsync(id, cancellationToken);
        if (model == null) {
            return ViewWithError(_IndexView, null, Core.ErrorInvalidId);
        }
        return View(_CreateEditView, model);
    }

    /// <summary>
    /// Saves updated user if valid. Renders the edit page on error, or redirects to index page.
    /// </summary>
    [HttpPut, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UserModel model, CancellationToken cancellationToken = default) => await Save(model, cancellationToken);

    /// <summary>
    /// Deletes the user if valid, and renders the index page.
    /// </summary>
    [HttpDelete]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default) {
        if (!await _UserService.DeleteUserAsync(id, cancellationToken)) {
            return ViewWithError(_IndexView, null, Users.ErrorDeletingUser);
        }

        ViewData.AddMessage(Users.SuccessDeletingUser);
        return IndexWithPushState();
    }

    /// <summary>
    /// Save the model using the user service.  Used by Create and Edit.
    /// </summary>
    private async Task<IActionResult> Save(UserModel model, CancellationToken cancellationToken = default) {
        if (!ModelState.IsValid) {
            return ViewWithError(_CreateEditView, model, ModelState);
        }

        (var success, var message) = await _UserService.SaveUserAsync(model, cancellationToken);
        if (!success) {
            return ViewWithError(_CreateEditView, model, message);
        }

        ViewData.AddMessage(message);
        return IndexWithPushState();
    }

    /// <summary>
    /// Helper method to add a header to pushState to Index and return the Index action.
    /// </summary>
    private IActionResult IndexWithPushState() {
        AddPushState(nameof(Index));
        return Index();
    }
}
