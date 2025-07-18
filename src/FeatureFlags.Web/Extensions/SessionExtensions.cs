using System.Data;

namespace FeatureFlags.Extensions;

public static class SessionExtensions {
    /// <summary>
    /// Check if a boolean session setting is true.
    /// </summary>
    /// <param name="session">Session to check.</param>
    /// <param name="settingName">Name of setting to check for.</param>
    /// <returns>True if setting is enabled, else false.</returns>
    public static bool IsEnabled(this ISession session, string settingName)
        => session.GetString(settingName)?.ToBool() == true;

    /// <summary>
    /// Toogle a boolean session setting.
    /// </summary>
    /// <param name="session">Session to modify.</param>
    /// <param name="settingName">Name of setting to toggle.</param>
    public static void ToggleSetting(this ISession session, string settingName)
        => session.SetString(settingName, (!session.IsEnabled(settingName)).ToString());
}
