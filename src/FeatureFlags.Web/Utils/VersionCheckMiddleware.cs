using System.Reflection;
using FeatureFlags.Constants;

namespace FeatureFlags.Utils;

/// <summary>
/// Checks that the requested version matches the current app version and adds the refresh header to the response if not.
/// </summary>
public sealed class VersionCheckMiddleware(RequestDelegate next) {
    private static readonly string? _VersionNumber = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "";
    private readonly RequestDelegate _Next = next;

    /// <summary>
    /// Processes the incoming HTTP request and conditionally sets a response header to indicate a refresh is required.
    /// </summary>
    /// <param name="context"><see cref="HttpContext"/> representing the current HTTP request and response.</param>
    /// <returns><see cref="Task"/> that represents the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context) {
        var acceptVersion = context.Request.Headers[PJax.Version].ToString();
        if (!string.IsNullOrWhiteSpace(acceptVersion) && _VersionNumber != acceptVersion) {
            context.Response.Headers[PJax.Refresh] = "true";
        }
        await _Next(context);
    }
}
