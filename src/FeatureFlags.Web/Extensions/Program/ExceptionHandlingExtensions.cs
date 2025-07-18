using System.Text.Json;
using FeatureFlags.Controllers;

namespace FeatureFlags.Extensions.Program;

public static class ExceptionHandlingExtensions {
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app, WebApplicationBuilder builder) {
        if (builder.Environment.IsDevelopment()) {
            app.UseDeveloperExceptionPage();
            return app;
        }

        app.UseExceptionHandler(builder => builder.Run(async context => {
            if (context.Request.ContentType?.Contains("json", StringComparison.OrdinalIgnoreCase) == true) {
                context.Response.StatusCode = (int)System.Net.HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";

                using var writer = new StreamWriter(context.Response.Body);
                writer.Write(JsonSerializer.Serialize(new { Error = "An unexpected error occurred." }));
                await writer.FlushAsync().ConfigureAwait(false);
            } else {
                context.Response.Redirect($"/{nameof(ErrorController).StripController()}/{nameof(ErrorController.Index)}");
            }
        }));
        return app;
    }
}
