using FeatureFlags.Attributes;
using FeatureFlags.Exceptions;
using FeatureFlags.Extensions;
using FeatureFlags.Models;
using FeatureFlags.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FeatureFlags.Controllers;

/// <summary>
/// Provides actions for handling errors and logging client-side JavaScript errors.
/// </summary>
/// <remarks>This controller is accessible without authentication and includes actions for displaying error views
/// and logging JavaScript errors from client-side applications.</remarks>
[AllowAnonymous]
public class ErrorController(ILogger<ErrorController> logger) : BaseController(logger) {
    private const string _ErrorView = "Error";

    /// <summary>
    /// Returns the error view and logs an error if a code is provided.
    /// </summary>
    /// <param name="code">An optional error code to log. If not null or whitespace, the error is logged with the original request path.</param>
    /// <returns>An <see cref="IActionResult"/> that renders the error view.</returns>
    public IActionResult Index(string? code = null) {
        if (!string.IsNullOrWhiteSpace(code)) {
            Logger.LogError(Core.UnhandledError, code, HttpContext.Features.Get<IStatusCodeReExecuteFeature>()?.OriginalPath);
        }

        if (HttpContext.Request.IsAjaxRequest()) {
            return StatusCode(StatusCodes.Status400BadRequest, Core.ErrorGeneric);
        }

        return View(_ErrorView);
    }

    /// <summary>
    /// Returns the Access Denied view with a generic error message.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> representing the Access Denied view.</returns>
    public IActionResult AccessDenied() {
        if (HttpContext.Request.IsAjaxRequest()) {
            return StatusCode(StatusCodes.Status403Forbidden, Core.ErrorAccessDenied);
        }

        ViewData.AddMessage(Core.ErrorGeneric);
        return View(_ErrorView);
    }

    /// <summary>
    /// Logs javascript errors from the client.
    /// </summary>
    /// <param name="error">Error details from the client.</param>
    /// <remarks>Ajax only to make a spamming a bit harder.</remarks>
    [HttpPost, AjaxRequestOnly]
    public IActionResult LogJavascriptError(JavascriptError error) {
        Logger.LogError(new JavascriptException(error.Message), "{Message}\n    at {Url}\n{Stack}", error.Message, error.Url, error.Stack);
        return Ok();
    }
}
