using FeatureFlags.Constants;
using FeatureFlags.Services;
using FeatureFlags.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;

namespace FeatureFlags.Controllers;

/// <summary>
/// Provides API endpoints for retrieving feature flag definitions.
/// </summary>
[Authorize(AuthenticationSchemes = ApiKeyAuthenticationOptions.AuthenticationScheme)]
[ApiController, Route("api"), ApiExplorerSettings(GroupName = Swagger.GroupName)]
public class FeatureFlagApiController(IFeatureFlagService featureFlagService) : ControllerBase {
    private readonly IFeatureFlagService _FeatureFlagService = featureFlagService;

    /// <summary>
    /// Get all feature flag definitions.
    /// </summary>
    [HttpGet("features")]
    [ProducesResponseType(typeof(IEnumerable<FeatureDefinition>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllFeatureDefinitionsAsync(CancellationToken cancellationToken = default) {
        var featureFlags = (await _FeatureFlagService.GetAllFeatureFlagsAsync(cancellationToken)).Where(x => x.Status);
        var definitions = featureFlags.Select(CustomFeatureDefinitionMapper.MapToCustomFeatureDefinition);
        return Ok(definitions);
    }
}
