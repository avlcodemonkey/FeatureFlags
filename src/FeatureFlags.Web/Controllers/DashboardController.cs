using Microsoft.AspNetCore.Mvc;

namespace FeatureFlags.Controllers;

public class DashboardController(ILogger<DashboardController> logger) : BaseController(logger) {
    public IActionResult Index() => View("Index");
}
