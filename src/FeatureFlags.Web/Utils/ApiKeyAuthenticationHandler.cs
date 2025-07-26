using System.Security.Claims;
using System.Text.Encodings.Web;
using FeatureFlags.Constants;
using FeatureFlags.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace FeatureFlags.Utils;

/// <summary>
/// Handles API key authentication for incoming HTTP requests.
/// </summary>
/// <remarks>This handler extracts the API key from the request headers and validates it using the provided <see
/// cref="IApiKeyService"/>. If the API key is valid, it creates an authenticated user identity.</remarks>
/// <param name="apiKeyService">The service used to validate API keys.</param>
/// <param name="options">The options monitor for <see cref="ApiKeyAuthenticationOptions"/>.</param>
/// <param name="logger">The logger factory used for logging operations.</param>
/// <param name="encoder">The URL encoder used for encoding operations.</param>
public class ApiKeyAuthenticationHandler(IApiKeyService apiKeyService, IOptionsMonitor<ApiKeyAuthenticationOptions> options,
    ILoggerFactory logger, UrlEncoder encoder) : AuthenticationHandler<ApiKeyAuthenticationOptions>(options, logger, encoder) {

    /// <summary>
    /// Authenticates the request using an API key provided in the request headers.
    /// </summary>
    /// <remarks>This method checks for the presence of a single API key in the request headers. If the key is valid, it creates
    /// a claims identity and returns a successful authentication result. Otherwise, it returns a failure result.</remarks>
    /// <returns>An <see cref="AuthenticateResult"/> indicating the success or failure of the authentication process.</returns>
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync() {
        if (!Request.Headers.TryGetValue(ApiKeyAuthenticationOptions.HeaderName, out var apiKeyHeaders) || apiKeyHeaders.Count != 1) {
            return AuthenticateResult.Fail("Invalid parameters");
        }

        var keyValue = apiKeyHeaders[0]?.ToString().Trim() ?? "";
        if (string.IsNullOrEmpty(keyValue)) {
            return AuthenticateResult.Fail("Invalid parameters");
        }

        var apiKey = await apiKeyService.GetApiKeyByKeyAsync(keyValue);
        if (apiKey == null) {
            return AuthenticateResult.Fail("Invalid parameters");
        }

        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, apiKey.Name) }, ApiKeyAuthenticationOptions.AuthenticationScheme);
        return AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(new List<ClaimsIdentity> { identity }),
            ApiKeyAuthenticationOptions.AuthenticationScheme));
    }
}
