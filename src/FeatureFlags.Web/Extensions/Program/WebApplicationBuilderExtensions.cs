namespace FeatureFlags.Extensions.Program;

public static class WebApplicationBuilderExtensions {
    /// <summary>
    /// Configures logging for the application.
    /// </summary>
    public static WebApplicationBuilder ConfigureLogging(this WebApplicationBuilder builder) {
        builder.Logging.ClearProviders();

        if (builder.Environment.IsDevelopment()) {
            // in development, log to console
            builder.Logging.AddSimpleConsole(options => {
                options.IncludeScopes = true;
                options.SingleLine = false;
            });
        } else {
            // in production, log to sentry

            // send anything this level or higher to sentry as an event instead of a breadcrumb
            // if we can't read the default log level, it'll default to Error
            // set this to Warning or Error (default) in non-development environments
            var logLevel = ParseLogLevel(builder.Configuration["Logging:LogLevel:Default"]);

            builder.WebHost.UseSentry(options => {
                if (logLevel != null) {
                    options.MinimumEventLevel = logLevel.Value;
                }
            });
            builder.Logging.AddSentry();
        }

        return builder;
    }

    /// <summary>
    /// Converts a string to a LogLevel enum value. Returns null if conversion fails.
    /// </summary>
    private static LogLevel? ParseLogLevel(string? value) {
        if (Enum.TryParse<LogLevel>(value, ignoreCase: true, out var level)) {
            return level;
        }
        return null;
    }
}
