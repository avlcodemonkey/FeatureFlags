using FeatureFlags.Constants;

namespace FeatureFlags.Utils;

/// <summary>
/// Adds additional details to logging.
/// </summary>
public sealed class LogEnricherMiddleware(RequestDelegate next, ILogger<LogEnricherMiddleware> logger) {
    private readonly RequestDelegate _Next = next;
    private readonly ILogger<LogEnricherMiddleware> _Logger = logger;

    /// <summary>
    /// Processes the incoming HTTP request and passes it to the next middleware in the pipeline.
    /// </summary>
    /// <param name="context"><see cref="HttpContext"/> representing the current HTTP request.</param>
    /// <returns><see cref="Task"/> that represents the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context) {
        var userId = context.User.FindFirst(Auth.UserIdClaim)?.Value;
        if (!string.IsNullOrEmpty(userId)) {
            using (_Logger.BeginScope("UserId:{UserId}", userId)) {
                await _Next(context);
            }
        } else {
            await _Next(context);
        }
    }
}
