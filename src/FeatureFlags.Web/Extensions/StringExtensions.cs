namespace FeatureFlags.Extensions;

/// <summary>
/// Provides extension methods for string manipulation, including conversions and keyword stripping.
/// </summary>
public static class StringExtensions {
    /// <summary>
    /// Converts a string value to a boolean. Default to false.
    /// </summary>
    /// <param name="val">Value to attempt to convert.</param>
    /// <returns>Bool value</returns>
    public static bool ToBool(this string? val) => val != null && (val == "1" || val.Equals("true", StringComparison.InvariantCultureIgnoreCase));


    /// <summary>
    /// Strip the `Controller` keyword from the end of a string for use with redirects.
    /// </summary>
    /// <param name="value">Controller name to strip.</param>
    /// <returns>Shortened controller name.</returns>
    public static string StripController(this string value) {
        var i = value.LastIndexOf("Controller", StringComparison.InvariantCultureIgnoreCase);
        return i > -1 ? value[..i] : value;
    }

    /// <summary>
    /// Strip the `Model` keyword from the end of a string for use with links.
    /// </summary>
    /// <param name="value">Model name to strip.</param>
    /// <returns>Shortened model name.</returns>
    public static string StripModel(this string value) {
        var i = value.LastIndexOf("Model", StringComparison.InvariantCultureIgnoreCase);
        return i > -1 ? value[..i] : value;
    }
}
