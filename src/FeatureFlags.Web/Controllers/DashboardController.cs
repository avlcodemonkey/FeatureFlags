using Microsoft.AspNetCore.Mvc;

namespace FeatureFlags.Controllers;

/// <summary>
/// Provides actions for rendering the dashboard view.
/// </summary>
public class DashboardController(ILogger<DashboardController> logger) : BaseController(logger) {
    /// <summary>
    /// Renders the landing page.
    /// </summary>
    public IActionResult Index() => View("Index");
}
