using FeatureFlags.Constants;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace FeatureFlags.Utils;

public sealed class LifetimeCookieAuthenticationEvents : CookieAuthenticationEvents {
    private readonly long _LifeTimeExpirationTicks = TimeSpan.FromMinutes(Auth.AbsoluteCookieLifeTime).Ticks;

    public const string CookieIssuedTicks = nameof(CookieIssuedTicks);

    public override async Task SigningIn(CookieSigningInContext context) {
        context.Properties.SetString(CookieIssuedTicks, DateTimeOffset.UtcNow.Ticks.ToString());

        await base.SigningIn(context);
    }

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
