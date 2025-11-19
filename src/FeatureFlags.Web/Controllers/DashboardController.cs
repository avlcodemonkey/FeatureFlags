using FeatureFlags.Attributes;
using FeatureFlags.Models;
using FeatureFlags.Resources;
using FeatureFlags.Services;
using Microsoft.AspNetCore.Mvc;

namespace FeatureFlags.Controllers;

/// <summary>
/// Provides actions for rendering the dashboard view.
/// </summary>
public class DashboardController(IApiRequestService apiRequestService, IUserService userService, ILogger<DashboardController> logger) : BaseController(logger) {
    private readonly IApiRequestService _ApiRequestService = apiRequestService;
    private readonly IUserService _UserService = userService;

    private DashboardModel GetBaseModel() => new() {
        RequestsByApiKey = new ChartModel {
            Title = Dashboard.TopApiKeys,
            SourceUrl = Url.Action(nameof(RequestsByApiKey))!
        },
        RequestsByIpAddress = new ChartModel {
            Title = Dashboard.TopIpAddresses,
            SourceUrl = Url.Action(nameof(RequestsByIpAddress))!
        },
    };

    /// <summary>
    /// Renders the landing page.
    /// </summary>
    public IActionResult Index() => View("Index", GetBaseModel());

    /// <summary>
    /// Returns the chart data as json.
    /// </summary>
    [HttpGet, ParentAction(nameof(Index)), AjaxRequestOnly]
    public async Task<IActionResult> RequestsByApiKey(CancellationToken cancellationToken = default) {
        var user = await _UserService.GetUserByEmailAsync(User.Identity!.Name!, cancellationToken);
        if (user == null) {
            return BadRequest("User not found.");
        }

        var requests = await _ApiRequestService.GetApiRequestsByApiKeyAsync(user.Id, DateTime.UtcNow.AddDays(-30), DateTime.UtcNow, cancellationToken: cancellationToken);
        return Ok(requests.Select(x => new ChartDataModel {
            Label = x.Key,
            Value = x.Value.ToString(),
        }));
    }

    /// <summary>
    /// Returns the chart data as json.
    /// </summary>
    [HttpGet, ParentAction(nameof(Index)), AjaxRequestOnly]
    public async Task<IActionResult> RequestsByIpAddress(CancellationToken cancellationToken = default) {
        var user = await _UserService.GetUserByEmailAsync(User.Identity!.Name!, cancellationToken);
        if (user == null) {
            return BadRequest("User not found.");
        }

        var requests = await _ApiRequestService.GetApiRequestsByIpAddressAsync(user.Id, DateTime.UtcNow.AddDays(-30), DateTime.UtcNow, cancellationToken: cancellationToken);
        return Ok(requests.Select(x => new ChartDataModel {
            Label = x.Key,
            Value = x.Value.ToString(),
        }));
    }
}
