namespace FeatureFlags.Utils;

/// <summary>
/// Provides helper methods for working with user names.
/// </summary>
public static class NameHelper {
    /// <summary>
    /// Formats the user name as "name, email" with no whitespace.
    /// </summary>
    /// <param name="name">User name.</param>
    /// <param name="email">User email.</param>
    /// <returns>Formatted name to display.</returns>
    public static string DisplayName(string? name = null, string? email = null) {
        var trimmedName = name?.Trim();
        var trimmedEmail = email?.Trim();

        if (string.IsNullOrWhiteSpace(trimmedEmail)) {
            return trimmedName ?? "";
        }
        if (string.IsNullOrWhiteSpace(trimmedName)) {
            return trimmedEmail;
        }
        return $"{trimmedName} ({trimmedEmail})";
    }
}
