using System.Security.Claims;
using FeatureFlags.Utils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Moq;

namespace FeatureFlags.Web.Tests.Utils;

public class LifetimeCookieAuthenticationEventsTests {
    private readonly LifetimeCookieAuthenticationEvents _Events = new();
    private readonly CookieSigningInContext _SigningInContext;
    private readonly AuthenticationScheme _AuthenticationScheme = new("Cookies", null, typeof(CookieAuthenticationHandler));
    private readonly CookieAuthenticationOptions _CookieAuthenticationOptions = new();

    private readonly Mock<HttpContext> _MockHttpContext;
    private readonly Mock<IAuthenticationService> _MockAuthenticationService;
    private readonly Mock<IServiceProvider> _MockServiceProvider = new();

    public LifetimeCookieAuthenticationEventsTests() {
        _SigningInContext = new CookieSigningInContext(new Mock<HttpContext>().Object, _AuthenticationScheme, _CookieAuthenticationOptions, new ClaimsPrincipal(), null, new CookieOptions());

        // mock the HttpContext along with service provider
        _MockAuthenticationService = new Mock<IAuthenticationService>();
        _MockAuthenticationService.Setup(x => x.SignOutAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>())).Returns(Task.FromResult((object)null!));

        _MockServiceProvider.Setup(x => x.GetService(typeof(IAuthenticationService))).Returns(_MockAuthenticationService.Object);
        _MockHttpContext = new Mock<HttpContext>();
        _MockHttpContext.Setup(x => x.RequestServices).Returns(_MockServiceProvider.Object);
    }

    [Fact]
    public async Task SigningIn_ShouldSetCookieIssuedTicks() {
        // Act
        await _Events.SigningIn(_SigningInContext);

        // Assert
        var issuedTicks = _SigningInContext.Properties.GetString(nameof(LifetimeCookieAuthenticationEvents.CookieIssuedTicks));
        Assert.False(string.IsNullOrWhiteSpace(issuedTicks));
        Assert.True(long.TryParse(issuedTicks, out _));
    }

    [Fact]
    public async Task ValidatePrincipal_ShouldRejectPrincipal_WhenCookieIssuedTicksIsMissing() {
        // Arrange
        var validatePrincipalContext = new CookieValidatePrincipalContext(
            _MockHttpContext.Object,
            _AuthenticationScheme,
            new CookieAuthenticationOptions(),
            new AuthenticationTicket(new ClaimsPrincipal(), CookieAuthenticationDefaults.AuthenticationScheme)
        );

        // Act
        await _Events.ValidatePrincipal(validatePrincipalContext);

        // Assert
        Assert.Null(validatePrincipalContext.Principal);
        _MockAuthenticationService.Verify(x => x.SignOutAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>()), Times.Once);
    }

    [Fact]
    public async Task ValidatePrincipal_ShouldRejectPrincipal_WhenCookieIsExpired() {
        // Arrange
        var expiredTicks = (DateTimeOffset.UtcNow - TimeSpan.FromHours(9)).Ticks.ToString();
        var validatePrincipalContext = new CookieValidatePrincipalContext(
            _MockHttpContext.Object,
            _AuthenticationScheme,
            new CookieAuthenticationOptions(),
            new AuthenticationTicket(new ClaimsPrincipal(), CookieAuthenticationDefaults.AuthenticationScheme)
        );
        validatePrincipalContext.Properties.SetString(LifetimeCookieAuthenticationEvents.CookieIssuedTicks, expiredTicks);

        // Act
        await _Events.ValidatePrincipal(validatePrincipalContext);

        // Assert
        Assert.Null(validatePrincipalContext.Principal);
        _MockAuthenticationService.Verify(x => x.SignOutAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>()), Times.Once);
    }

    [Fact]
    public async Task ValidatePrincipal_ShouldNotRejectPrincipal_WhenCookieIsValid() {
        // Arrange
        var validTicks = DateTimeOffset.UtcNow.Ticks.ToString();
        var validatePrincipalContext = new CookieValidatePrincipalContext(
            _MockHttpContext.Object,
            _AuthenticationScheme,
            new CookieAuthenticationOptions(),
            new AuthenticationTicket(new ClaimsPrincipal(), CookieAuthenticationDefaults.AuthenticationScheme)
        );
        validatePrincipalContext.Properties.SetString(LifetimeCookieAuthenticationEvents.CookieIssuedTicks, validTicks);

        // Act
        await _Events.ValidatePrincipal(validatePrincipalContext);

        // Assert
        Assert.NotNull(validatePrincipalContext.Principal);
        _MockAuthenticationService.Verify(x => x.SignOutAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>()), Times.Never);
    }
}
