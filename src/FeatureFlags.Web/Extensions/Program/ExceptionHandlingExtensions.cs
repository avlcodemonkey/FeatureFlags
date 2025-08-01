using System.Text.Json;
using FeatureFlags.Controllers;

namespace FeatureFlags.Extensions.Program;

/// <summary>
/// Configures exception handling middleware for the application.
/// </summary>
public static class ExceptionHandlingExtensions {
    /// <summary>
    /// Configures exception handling middleware for the application.
    /// </summary>
    /// <remarks>In development environments, this method adds the Developer Exception Page middleware to
    /// display detailed error information. In non-development environments, it configures a custom exception handler
    /// that: <list type="bullet"> <item> Redirects requests with a non-JSON content type to a generic error page.
    /// </item> <item> Returns a JSON-formatted error response for requests with a JSON content type. </item>
    /// </list></remarks>
    /// <param name="app"><see cref="IApplicationBuilder"/> instance used to configure the application's request pipeline.</param>
    /// <param name="builder"><see cref="WebApplicationBuilder"/> instance providing access to the application's environment and configuration.</param>
    /// <returns>The <see cref="IApplicationBuilder"/> instance with the exception handling middleware configured.</returns>
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
