namespace FeatureFlags.Constants;

public enum FeatureFlags {
    UserRegistration = 0,
    UnitedStatesOnly = 1,
}

public static class FeatureFlagConstants {

    /// <summary>
    /// Cache life span in minutes, absolute expiration.
    /// </summary>
    public const int CacheLifeTime = 60;
}
