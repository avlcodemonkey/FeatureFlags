using Microsoft.AspNetCore.Authentication;

namespace FeatureFlags.Constants;

/// <summary>
/// Provides options for configuring API key authentication.
/// </summary>
public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions {
    /// <summary>
    /// Represents the default authentication scheme used for API key authentication.
    /// </summary>
    public const string AuthenticationScheme = "ApiKey";

    /// <summary>
    /// Represents the name of the HTTP header used for API key authentication.
    /// </summary>
    public const string HeaderName = "x-api-key";
}
