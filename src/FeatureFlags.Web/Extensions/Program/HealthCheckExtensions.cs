using System.Text;
using System.Text.Json;
using FeatureFlags.Domain;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FeatureFlags.Extensions.Program;

public static class HealthCheckExtensions {
    public static IServiceCollection AddCustomHealthChecks(this IServiceCollection services) {
        services
            .AddHealthChecks()
            .AddDbContextCheck<FeatureFlagsDbContext>();

        return services;
    }

    public static IApplicationBuilder UseHealthChecks(this IApplicationBuilder app) {
        ((WebApplication)app).MapHealthChecks("/status", new HealthCheckOptions {
            ResponseWriter = WriteResponse
        });

        return app;
    }

    /// <summary>
    /// Output health status as detailed JSON.
    /// </summary>
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
