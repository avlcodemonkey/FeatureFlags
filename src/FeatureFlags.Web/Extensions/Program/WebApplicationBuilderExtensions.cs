namespace FeatureFlags.Extensions.Program;

/// <summary>
/// Provides extension methods for configuring a <see cref="WebApplicationBuilder"/>.
/// </summary>
public static class WebApplicationBuilderExtensions {
    /// <summary>
    /// Configures logging for the application.
    /// </summary>
    public static WebApplicationBuilder ConfigureLogging(this WebApplicationBuilder builder) {
        builder.Logging.ClearProviders();

        // in a cloud environment logging to console is normally the simplest option
        builder.Logging.AddSimpleConsole(options => {
            options.IncludeScopes = true;
            options.SingleLine = false;
        });

        return builder;
    }
}
