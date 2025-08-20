using Serilog;
using Serilog.Formatting.Compact;

namespace FeatureFlags.Extensions.Program;

/// <summary>
/// Provides extension methods for configuring a <see cref="WebApplicationBuilder"/>.
/// </summary>
public static class WebApplicationBuilderExtensions {
    /// <summary>
    /// Configures logging for the application.
    /// </summary>
    /// <remarks>
    /// This is very simple for now, but it can be expanded later as needed.
    /// </remarks>
    public static WebApplicationBuilder ConfigureLogging(this WebApplicationBuilder builder) {
        var isDevelopment = builder.Environment.IsDevelopment();

        builder.Services.AddSerilog((services, loggerConfiguration) => {
            loggerConfiguration.ReadFrom.Configuration(builder.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext();

            if (isDevelopment) {
                // Use default console formatting for development
                loggerConfiguration.WriteTo.Console();
            } else {
                // Use CompactJsonFormatter for non-development environments
                loggerConfiguration.WriteTo.Console(new CompactJsonFormatter());
            }
        });

        return builder;
    }
}
