namespace FeatureFlags.Constants;

/// <summary>
/// Specifies the type of requirement logic to be applied when evaluating conditions.
/// </summary>
public enum RequirementType {
    /// <summary>
    /// Represents a value indicating that any option is acceptable.
    /// </summary>
    Any = 1,

    /// <summary>
    /// Represents a value indicating that all options are required.
    /// </summary>
    All = 2
}
