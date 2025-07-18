using FeatureFlags.Constants;

namespace FeatureFlags.Utils;

/// <summary>
/// Adds additional details to logging.
/// </summary>
public sealed class LogEnricherMiddleware(RequestDelegate next, ILogger<LogEnricherMiddleware> logger) {
    private readonly RequestDelegate _Next = next;
    private readonly ILogger<LogEnricherMiddleware> _Logger = logger;

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
