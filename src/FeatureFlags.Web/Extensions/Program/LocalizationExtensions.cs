using System.Globalization;
using Microsoft.AspNetCore.Localization;

namespace FeatureFlags.Extensions.Program;

public static class LocalizationExtensions {
    private static readonly List<CultureInfo> _Cultures = [new CultureInfo("en"), new CultureInfo("es")];

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
