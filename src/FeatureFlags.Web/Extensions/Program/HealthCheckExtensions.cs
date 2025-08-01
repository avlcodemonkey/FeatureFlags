using System.Text;
using System.Text.Json;
using FeatureFlags.Domain;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FeatureFlags.Extensions.Program;

/// <summary>
/// Provides extension methods for configuring and using health checks.
/// </summary>
public static class HealthCheckExtensions {
    /// <summary>
    /// Adds custom health checks to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/> to which the health checks are added.</param>
    /// <returns>Updated <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddCustomHealthChecks(this IServiceCollection services) {
        services
            .AddHealthChecks()
            .AddDbContextCheck<FeatureFlagsDbContext>();

        return services;
    }

    /// <summary>
    /// Adds health check middleware to the application's request pipeline.
    /// </summary>
    /// <param name="app"><see cref="IApplicationBuilder"/> instance to configure.</param>
    /// <returns><see cref="IApplicationBuilder"/> instance, allowing for further configuration.</returns>
    public static IApplicationBuilder UseHealthChecks(this IApplicationBuilder app) {
        ((WebApplication)app).MapHealthChecks("/status", new HealthCheckOptions {
            ResponseWriter = WriteResponse
        });

        return app;
    }

    /// <summary>
    /// Output health status as detailed JSON.
    /// </summary>
    /// <param name="context"><see cref="HttpContext"/> for the current request.</param>
    /// <param name="healthReport"><see cref="HealthReport"/> containing the health check results.</param>
    private static Task WriteResponse(HttpContext context, HealthReport healthReport) {
        context.Response.ContentType = "application/json; charset=utf-8";

        using var memoryStream = new MemoryStream();
        using (var jsonWriter = new Utf8JsonWriter(memoryStream, new JsonWriterOptions { Indented = true })) {
            jsonWriter.WriteStartObject();
            jsonWriter.WriteString("status", healthReport.Status.ToString());
            jsonWriter.WriteStartObject("results");

            foreach (var healthReportEntry in healthReport.Entries) {
                jsonWriter.WriteStartObject(healthReportEntry.Key);
                jsonWriter.WriteString("status", healthReportEntry.Value.Status.ToString());
                jsonWriter.WriteString("description", healthReportEntry.Value.Description);
                jsonWriter.WriteStartObject("data");

                foreach (var item in healthReportEntry.Value.Data) {
                    jsonWriter.WritePropertyName(item.Key);
                    JsonSerializer.Serialize(jsonWriter, item.Value, item.Value?.GetType() ?? typeof(object));
                }

                jsonWriter.WriteEndObject();
                jsonWriter.WriteEndObject();
            }

            jsonWriter.WriteEndObject();
            jsonWriter.WriteEndObject();
        }

        return context.Response.WriteAsync(Encoding.UTF8.GetString(memoryStream.ToArray()));
    }
}
