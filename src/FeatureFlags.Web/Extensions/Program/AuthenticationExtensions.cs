using FeatureFlags.Constants;
using FeatureFlags.Controllers;
using FeatureFlags.Utils;
using Microsoft.AspNetCore.Authorization;

namespace FeatureFlags.Extensions.Program;

/// <summary>
/// Provides extension methods for configuring authentication and session management using Auth0.
/// </summary>
public static class AuthenticationExtensions {
    /// <summary>
    /// Configure authentication and session for app using Auth0.
    /// </summary>
    public static IServiceCollection AddCookieAuthentication(this IServiceCollection services) {
        services.AddAuthentication()
            // register the API key authentication scheme for API access
            .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(ApiKeyAuthenticationOptions.AuthenticationScheme, null)
            // register the cookie authentication scheme for web access
            .AddCookie(options => {
                options.ExpireTimeSpan = TimeSpan.FromMinutes(Auth.RollingCookieLifeTime);
                options.SlidingExpiration = true;
                options.AccessDeniedPath = $"/{nameof(ErrorController).StripController()}/{nameof(ErrorController.AccessDenied)}";
                options.LoginPath = $"/{nameof(AccountController).StripController()}/{nameof(AccountController.Login)}";
                options.LogoutPath = $"/{nameof(AccountController).StripController()}/{nameof(AccountController.Logout)}";
                options.EventsType = typeof(LifetimeCookieAuthenticationEvents);
            });

        services.AddCookiePolicy(options => {
            options.Secure = CookieSecurePolicy.Always;
            options.HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always;
            options.MinimumSameSitePolicy = SameSiteMode.Strict;
        });

        services.AddAuthorization(options => options.AddPolicy(PermissionRequirementHandler.PolicyName,
            policy => policy.Requirements.Add(new PermissionRequirement())));
        services.AddSingleton<IAuthorizationHandler, PermissionRequirementHandler>();
        services.AddTransient<LifetimeCookieAuthenticationEvents>();

        services.AddDataProtection();

        return services;
    }
}
