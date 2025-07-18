using FeatureFlags.Constants;
using FeatureFlags.Extensions;
using FeatureFlags.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FeatureFlags.Controllers;

[Authorize(Policy = PermissionRequirementHandler.PolicyName)]
public abstract class BaseController(ILogger<Controller> logger) : Controller {
    protected ILogger<Controller> Logger { get; set; } = logger;

    /// <summary>
    /// Adds a PushUrl header to the response that the client pushes into browser history.
    /// </summary>
    /// <param name="action">Action to generate a URL for. Url is pushed to client.</param>
    [NonAction]
    public void AddPushState(string action) {
        if (!string.IsNullOrWhiteSpace(action)) {
            Response.Headers.Append(PJax.PushUrl, Url.Action(action));
        }
    }

    /// <summary>
    /// Returns a view while also adding a message in viewData.
    /// </summary>
    /// <param name="viewName">The name or path of the view that is rendered to the response.</param>
    /// <param name="model">Model that is rendered by the view.</param>
    /// <param name="message">Message to add to viewData.</param>
    [NonAction]
    public ViewResult ViewWithMessage(string viewName, object? model, string message) {
        ViewData.AddMessage(message);
        return View(viewName, model);
    }

    /// <summary>
    /// Returns a view while also adding an error message in viewData.
    /// </summary>
    /// <param name="viewName">The name or path of the view that is rendered to the response.</param>
    /// <param name="model">Model that is rendered by the view.</param>
    /// <param name="error">Error message to add to viewData.</param>
    [NonAction]
    public ViewResult ViewWithError(string viewName, object? model, string error) {
        ViewData.AddError(error);
        return View(viewName, model);
    }

    /// <summary>
    /// Returns a view while also adding an error message in viewData.
    /// </summary>
    /// <param name="viewName">The name or path of the view that is rendered to the response.</param>
    /// <param name="model">Model that is rendered by the view.</param>
    /// <param name="modelState">ModelStateDictionary to build error message from.</param>
    [NonAction]
    public ViewResult ViewWithError(string viewName, object? model, ModelStateDictionary modelState) {
        ViewData.AddError(modelState);
        return View(viewName, model);
    }
}
