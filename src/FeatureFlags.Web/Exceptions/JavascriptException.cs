namespace FeatureFlags.Exceptions;

/// <summary>
/// Represents an exception that occurs during the execution of JavaScript code.
/// </summary>
public class JavascriptException : Exception {
    /// <summary>
    /// Represents an exception that occurs during the execution of JavaScript code.
    /// </summary>
    /// <param name="message">Error message that describes the reason for the exception.</param>
    public JavascriptException(string message) : base(message) { }

    /// <summary>
    /// Represents an exception that occurs during the execution of JavaScript code.
    /// </summary>
    public JavascriptException() { }

    /// <summary>
    /// Represents an exception that occurs during the execution of JavaScript code.
    /// </summary>
    /// <param name="message">Error message that describes the reason for the exception.</param>
    /// <param name="innerException">Exception that is the cause of the current exception.</param>
    public JavascriptException(string message, Exception innerException) : base(message, innerException) { }
}
