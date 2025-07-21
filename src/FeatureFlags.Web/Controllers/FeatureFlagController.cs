using FeatureFlags.Attributes;
using FeatureFlags.Client;
using FeatureFlags.Extensions;
using FeatureFlags.Models;
using FeatureFlags.Resources;
using FeatureFlags.Services;
using FeatureFlags.Utils;
using Microsoft.AspNetCore.Mvc;

namespace FeatureFlags.Controllers;

public class FeatureFlagController(IFeatureFlagService featureFlagService, IFeatureFlagClient featureFlagClient, ILogger<FeatureFlagController> logger)
    : BaseController(logger) {

    private readonly IFeatureFlagService _FeatureFlagService = featureFlagService;
    private readonly IFeatureFlagClient _FeatureFlagClient = featureFlagClient;

    private const string _IndexView = "Index";

    /// <summary>
    /// Renders the feature flag landing page with the table of flags.
    /// </summary>
    [HttpGet]
    public IActionResult Index() => View(_IndexView);

    /// <summary>
    /// Returns the feature flag list as json.
    /// </summary>
    [HttpGet, ParentAction(nameof(Index)), AjaxRequestOnly]
    public async Task<IActionResult> List(CancellationToken cancellationToken = default)
        => Ok((await _FeatureFlagService.GetAllFeatureFlagsAsync(cancellationToken))
            .Select(x => new FeatureFlagListResultModel { Id = x.Id, Name = x.Name, IsEnabled = x.IsEnabled }));

    /// <summary>
    /// Enables a flag if valid. Renders the index page with error or success message.
    /// </summary>
    [HttpPatch]
    public async Task<IActionResult> Enable(int id, CancellationToken cancellationToken = default) {
        var featureFlagModel = await _FeatureFlagService.GetFeatureFlagByIdAsync(id, cancellationToken);
        if (featureFlagModel == null) {
            ViewData.AddError(Core.ErrorInvalidId);
            return IndexWithPushState();
        }

        var (success, message) = await _FeatureFlagService.SaveFeatureFlagAsync(featureFlagModel with { IsEnabled = true }, cancellationToken);
        if (!success) {
            ViewData.AddError(message);
            return IndexWithPushState();
        }

        ViewData.AddMessage(Flags.SuccessSavingFlag);
        return IndexWithPushState();
    }

    /// <summary>
    /// Disables a flag if valid. Renders the index page with error or success message.
    /// </summary>
    [HttpPatch]
    public async Task<IActionResult> Disable(int id, CancellationToken cancellationToken = default) {
        var featureFlagModel = await _FeatureFlagService.GetFeatureFlagByIdAsync(id, cancellationToken);
        if (featureFlagModel == null) {
            ViewData.AddError(Core.ErrorInvalidId);
            return IndexWithPushState();
        }

        var (success, message) = await _FeatureFlagService.SaveFeatureFlagAsync(featureFlagModel with { IsEnabled = false }, cancellationToken);
        if (!success) {
            ViewData.AddError(message);
            return IndexWithPushState();
        }

        ViewData.AddMessage(Flags.SuccessSavingFlag);
        return IndexWithPushState();
    }

    /// <summary>
    /// Generates a list of all feature flags and sync's the database. Renders the index page.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> RefreshFlags(CancellationToken cancellationToken = default) {
        // @todo refresh should should be removed now
        var featureFlags = Enum.GetNames<Constants.InternalFeatureFlags>().ToDictionary(x => x.ToLower(), x => x);
        if (!await new FeatureFlagManager(_FeatureFlagService).RegisterAsync(featureFlags, cancellationToken)) {
            ViewData.AddError(Flags.ErrorRefreshingFlags);
            return IndexWithPushState();
        }

        ViewData.AddMessage(Flags.SuccessRefreshingFlags);
        return IndexWithPushState();
    }

    /// <summary>
    /// Clears the feature flag cache. Renders the index page.
    /// </summary>
    [HttpGet]
    public IActionResult ClearCache() {
        // @todo add tests for this
        _FeatureFlagClient.ClearCache();
        ViewData.AddMessage(Flags.SuccessClearingCache);
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
