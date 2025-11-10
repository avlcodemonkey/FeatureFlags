using FeatureFlags.Client;
using FeatureFlags.Constants;
using FeatureFlags.Services;
using FeatureFlags.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeatureFlags.Controllers;

/// <summary>
/// Provides API endpoints for retrieving feature flag definitions.
/// </summary>
[Authorize(AuthenticationSchemes = ApiKeyAuthenticationOptions.AuthenticationScheme)]
[ApiController, Route("api"), ApiExplorerSettings(GroupName = Swagger.GroupName)]
public class FeatureFlagApiController(IFeatureFlagService featureFlagService, IApiKeyService apiKeyService, IApiRequestService apiRequestService,
    ILogger<FeatureFlagApiController> logger) : ControllerBase {

    private readonly IFeatureFlagService _FeatureFlagService = featureFlagService;
    private readonly IApiKeyService _ApiKeyService = apiKeyService;
    private readonly IApiRequestService _ApiRequestService = apiRequestService;
    private readonly ILogger<FeatureFlagApiController> _Logger = logger;

    /// <summary>
    /// Get all feature flag definitions.
    /// </summary>
    [HttpGet("features")]
    [ProducesResponseType(typeof(IEnumerable<CustomFeatureDefinition>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllFeatureDefinitionsAsync([FromHeader(Name = Client.Constants.ApiKeyHeaderName)] string apiKeyHeader,
        [FromHeader(Name = Client.Constants.XForwardedForHeaderName)] string? xForwardedFor = null, CancellationToken cancellationToken = default) {

        // this should already be covered by the ApiKeyAuthenticationHandler, but double check
        if (string.IsNullOrEmpty(apiKeyHeader)) {
            return Unauthorized();
        }
        var apiKey = await _ApiKeyService.GetApiKeyByKeyAsync(apiKeyHeader, cancellationToken);
        if (apiKey == null) {
            return Unauthorized();
        }

        try {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            // Check for X-Forwarded-For header
            if (!string.IsNullOrEmpty(xForwardedFor)) {
                ipAddress = xForwardedFor;
            }

            await _ApiRequestService.SaveApiRequestAsync(apiKey.Id, ipAddress ?? "unknown", cancellationToken);
        } catch (Exception ex) {
            // Handle any errors that occur while saving the API request
            _Logger.LogError(ex, "Failed to log API request for API key ID {ApiKeyId}", apiKey.Id);
        }

        var featureFlags = await _FeatureFlagService.GetAllFeatureFlagsAsync(cancellationToken);
        return Ok(featureFlags.Select(CustomFeatureDefinitionMapper.MapToCustomFeatureDefinition));
    }
}
