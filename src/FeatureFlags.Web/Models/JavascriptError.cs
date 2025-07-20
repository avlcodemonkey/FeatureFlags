namespace FeatureFlags.Models;

public sealed record JavascriptError {
    public string Message { get; init; } = "";
    public string Url { get; init; } = "";
    public string Stack { get; init; } = "";
}
