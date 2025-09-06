namespace FeatureFlags.Constants;

/// <summary>
/// Filter types for feature flags.
/// </summary>
public enum FilterTypes {
    /// <summary>
    /// Targeting filter type.
    /// </summary>
    Targeting = 1,

    /// <summary>
    /// Time window filter type.
    /// </summary>
    TimeWindow = 2,

    /// <summary>
    /// Percentage filter type.
    /// </summary>
    Percentage = 3,

    /// <summary>
    /// JSON filter type.
    /// </summary>
    JSON = 4,
}
