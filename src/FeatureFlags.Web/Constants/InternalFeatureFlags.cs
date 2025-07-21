namespace FeatureFlags.Constants;

/// <summary>
/// Represents internal feature flags used to enable or disable specific application features.
/// </summary>
/// <remarks>This enumeration is intended for internal use only and is not designed for external API consumers.
/// Each flag corresponds to a specific feature that can be toggled on or off within the application.</remarks>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S2344:Enumeration type names should not have \"Flags\" or \"Enum\" suffixes", Justification = "Because this is for feature flags.")]
public enum InternalFeatureFlags {
    /// <summary>
    /// Feature flag for user registration functionality.
    /// </summary>
    /// <remarks>This flag controls whether user registration is enabled or disabled in the application and should not be removed.</remarks>
    UserRegistration = 0
}
