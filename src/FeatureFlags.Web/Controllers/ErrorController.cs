using FeatureFlags.Attributes;
using FeatureFlags.Exceptions;
using FeatureFlags.Extensions;
using FeatureFlags.Models;
using FeatureFlags.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FeatureFlags.Controllers;

[AllowAnonymous]
public class ErrorController(ILogger<ErrorController> logger) : BaseController(logger) {
    private const string _ErrorView = "Error";

    public IActionResult Index(string? code = null) {
        if (!string.IsNullOrWhiteSpace(code)) {
            Logger.LogError(Core.UnhandledError, code, HttpContext.Features.Get<IStatusCodeReExecuteFeature>()?.OriginalPath);
        }

        return View(_ErrorView);
    }

    public IActionResult AccessDenied() {
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
