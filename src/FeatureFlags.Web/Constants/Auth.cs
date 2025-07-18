namespace FeatureFlags.Constants;

/// <summary>
/// Authentication settings.
/// </summary>
public static class Auth {
    /// <summary>
    /// Length of token to generate for login.
    /// </summary>
    public const int TokenSize = 8;

    /// <summary>
    /// Token life span in minutes, absolute expiration.
    /// </summary>
    public const int TokenLifeTime = 15;

    /// <summary>
    /// Number of attempts to allow before invalidating token.
    /// </summary>
    public const int TokenMaxAttempts = 5;

    /// <summary>
    /// Cookie life span in minutes, sliding expiration.
    /// </summary>
    public const int RollingCookieLifeTime = 30;

    /// <summary>
    /// Cookie life span in minutes, absolute expiration.
    /// </summary>
    public const int AbsoluteCookieLifeTime = 480;

    /// <summary>
    /// Name of the auth claim containing the userId. Set during login and used for logging.
    /// </summary>
    public const string UserIdClaim = "UserId";
}
