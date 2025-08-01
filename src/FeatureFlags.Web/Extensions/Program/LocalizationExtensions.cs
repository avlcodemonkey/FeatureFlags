using System.Globalization;
using Microsoft.AspNetCore.Localization;

namespace FeatureFlags.Extensions.Program;

/// <summary>
/// Configures localization for the application, including supported cultures and request culture providers.
/// </summary>
public static class LocalizationExtensions {
    private static readonly List<CultureInfo> _Cultures = [new CultureInfo("en"), new CultureInfo("es")];

    /// <summary>
    /// Configures the application to use localization based on the specified cultures and request culture providers.
    /// </summary>
    /// <param name="app"><see cref="IApplicationBuilder"/> instance used to configure the application's request pipeline.</param>
    /// <returns><see cref="IApplicationBuilder"/> instance, enabling further configuration.</returns>
    public static IApplicationBuilder UseLocalization(this IApplicationBuilder app) {
        var localizationOptions = new RequestLocalizationOptions {
            DefaultRequestCulture = new RequestCulture("en"),
            SupportedCultures = _Cultures,
            SupportedUICultures = _Cultures,
        };
        localizationOptions.RequestCultureProviders.Insert(0, new CookieRequestCultureProvider() { });

        app.UseRequestLocalization(localizationOptions);

        return app;
    }
}
