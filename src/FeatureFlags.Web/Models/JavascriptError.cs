namespace FeatureFlags.Models;

/// <summary>
/// Represents a JavaScript error reported from the client.
/// </summary>
public sealed record JavascriptError {
    /// <summary>
    /// Gets the error message from the JavaScript exception.
    /// </summary>
    public string Message { get; init; } = "";

    /// <summary>
    /// Gets the URL where the error occurred.
    /// </summary>
    public string Url { get; init; } = "";

    /// <summary>
    /// Gets the stack trace associated with the JavaScript error.
    /// </summary>
    public string Stack { get; init; } = "";
}
