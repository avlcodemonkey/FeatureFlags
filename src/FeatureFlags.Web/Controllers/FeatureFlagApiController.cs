using FeatureFlags.Constants;
using FeatureFlags.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;

namespace FeatureFlags.Controllers;

/// <summary>
/// Provides API endpoints for retrieving feature flag definitions.
/// </summary>
/// <param name="featureFlagService">Service to load feature flags.</param>
[Authorize(AuthenticationSchemes = ApiKeyAuthenticationOptions.AuthenticationScheme)]
[ApiController, Route("api"), ApiExplorerSettings(GroupName = Swagger.GroupName)]
public class FeatureFlagApiController(IFeatureFlagService featureFlagService) : ControllerBase {

    private readonly IFeatureFlagService _FeatureFlagService = featureFlagService;

    /// <summary>
    /// Get all feature flag definitions.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token,</param>
    /// <returns>JSON feature definitions.</returns>
    [HttpGet("features")]
    [ProducesResponseType(typeof(IEnumerable<FeatureDefinition>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllFeatureDefinitionsAsync(CancellationToken cancellationToken = default) {
        var featureFlags = await _FeatureFlagService.GetAllFeatureFlagsAsync(cancellationToken);
        var definitions = featureFlags.Select(x => new FeatureDefinition {
            Name = x.Name,
            EnabledFor = x.IsEnabled ? new[] { new FeatureFilterConfiguration { Name = "AlwaysOn" } } : null
        });
        return Ok(definitions);
    }
}
