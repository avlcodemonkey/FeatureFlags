using FeatureFlags.Constants;
using FeatureFlags.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;

namespace FeatureFlags.Controllers;

/// <summary>
/// Provides API endpoints for managing and retrieving feature flag definitions.
/// </summary>
/// <remarks>This controller exposes endpoints to retrieve all feature flag definitions or a specific feature flag
/// definition by name. Feature flags are used to enable or disable application functionality dynamically.</remarks>
/// <param name="featureFlagService">Service to load feature flags.</param>
/// <param name="logger">Logger</param>
// @todo add api key authentication
[AllowAnonymous]
[ApiController, Route("api")]
[ApiExplorerSettings(GroupName = Swagger.GroupName)]
public class FeatureFlagApiController(IFeatureFlagService featureFlagService, ILogger<FeatureFlagApiController> logger)
    : BaseController(logger) {

    private readonly IFeatureFlagService _FeatureFlagService = featureFlagService;

    /// <summary>
    /// Get all feature flag definitions.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token,</param>
    /// <returns>JSON feature definitions.</returns>
    [HttpGet("features")]
    [ProducesResponseType(typeof(IEnumerable<FeatureDefinition>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAllFeatureDefinitionsAsync(CancellationToken cancellationToken = default) {
        var featureFlags = await _FeatureFlagService.GetAllFeatureFlagsAsync(cancellationToken);
        var definitions = featureFlags.Select(x => new FeatureDefinition {
            Name = x.Name,
            EnabledFor = x.IsEnabled ? new[] { new FeatureFilterConfiguration { Name = "AlwaysOn" } } : null
        });
        return Ok(definitions);
    }
}
