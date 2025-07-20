namespace FeatureFlags.Exceptions;

public class JavascriptException : Exception {
    public JavascriptException(string message) : base(message) { }

    public JavascriptException() { }

    public JavascriptException(string message, Exception innerException) : base(message, innerException) { }
}
