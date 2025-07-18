using System.Reflection;
using FeatureFlags.Constants;

namespace FeatureFlags.Utils;

/// <summary>
/// Checks that the requested version matches the current app version and adds the refresh header to the response if not.
/// </summary>
public sealed class VersionCheckMiddleware(RequestDelegate next) {
    private static readonly string? _VersionNumber = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "";
    private readonly RequestDelegate _Next = next;

    public async Task InvokeAsync(HttpContext context) {
        var acceptVersion = context.Request.Headers[PJax.Version].ToString();
        if (!string.IsNullOrWhiteSpace(acceptVersion) && _VersionNumber != acceptVersion) {
            context.Response.Headers[PJax.Refresh] = "true";
        }
        await _Next(context);
    }
}
