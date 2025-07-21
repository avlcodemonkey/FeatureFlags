namespace FeatureFlags.Client;

/// <summary>
/// Magic strings used throughout the feature flags client.
/// </summary>
public static class Constants {
    /// <summary>
    /// Name of the HTTP client used for feature flag requests.
    /// </summary>
    public const string HttpClientName = "FeatureFlagHttpClient";

    /// <summary>
    /// Name of the authentication scheme used for feature flag requests.
    /// </summary>
    public const string Bearer = "Bearer";
}
