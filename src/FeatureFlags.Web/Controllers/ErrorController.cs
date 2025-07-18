using FeatureFlags.Extensions;
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
}
