using FeatureFlags.Constants;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace FeatureFlags.Utils;

/// <summary>
/// Handles cookie authentication events to enforce an absolute cookie lifetime.
/// </summary>
public sealed class LifetimeCookieAuthenticationEvents : CookieAuthenticationEvents {
    /// <summary>
    /// Number of ticks representing the absolute cookie lifetime.
    /// </summary>
    private readonly long _LifeTimeExpirationTicks = TimeSpan.FromMinutes(Auth.AbsoluteCookieLifeTime).Ticks;

    /// <summary>
    /// Key used to store the issued ticks in authentication properties.
    /// </summary>
    public const string CookieIssuedTicks = nameof(CookieIssuedTicks);

    /// <summary>
    /// Called during the sign-in process to record the time the cookie was issued.
    /// </summary>
    /// <param name="context">Context for the sign-in event.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public override async Task SigningIn(CookieSigningInContext context) {
        context.Properties.SetString(CookieIssuedTicks, DateTimeOffset.UtcNow.Ticks.ToString());

        await base.SigningIn(context);
    }

    /// <summary>
    /// Called to validate the principal and enforce the absolute cookie lifetime.
    /// </summary>
    /// <param name="context">Context for the principal validation event.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context) {
        var cookieIssuedTicks = context.Properties.GetString(CookieIssuedTicks);

        if (string.IsNullOrWhiteSpace(cookieIssuedTicks)
            || !long.TryParse(cookieIssuedTicks, out var issuedTicks)
            || DateTimeOffset.UtcNow.Ticks - issuedTicks > _LifeTimeExpirationTicks
        ) {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync();
            return;
        }

        await base.ValidatePrincipal(context);
    }
}
