namespace FeatureFlags.Extensions.Program;

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
