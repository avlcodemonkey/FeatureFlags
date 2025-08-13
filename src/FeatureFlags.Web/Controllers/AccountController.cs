using System.Globalization;
using System.Security.Claims;
using FeatureFlags.Attributes;
using FeatureFlags.Constants;
using FeatureFlags.Extensions;
using FeatureFlags.Models;
using FeatureFlags.Resources;
using FeatureFlags.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

namespace FeatureFlags.Controllers;

/// <summary>
/// Provides functionality for managing user accounts, including registration, login, account updates, and logout.
/// </summary>
public class AccountController(IUserService userService, ILanguageService languageService, IEmailService emailService,
    IViewService viewService, IRoleService roleService, ILogger<AccountController> logger) : BaseController(logger) {

    private readonly ILanguageService _LanguageService = languageService;
    private readonly IUserService _UserService = userService;
    private readonly IEmailService _EmailService = emailService;
    private readonly IViewService _ViewService = viewService;
    private readonly IRoleService _RoleService = roleService;

    private const string _RegisterView = "Register";
    private const string _LoginView = "Login";
    private const string _TokenEmailView = "TokenEmail";
    private const string _LoginTokenView = "LoginToken";
    private const string _IndexView = "Index";
    private const string _ToggleContextHelpView = "ToggleContextHelp";
    private const string _UpdateAccountView = "UpdateAccount";

    private bool IsAuthenticated => User.Identity?.IsAuthenticated == true;
    private RedirectToActionResult RedirectToDashboard => RedirectToAction(nameof(DashboardController.Index), nameof(DashboardController).StripController());

    #region Anonymous Endpoints

    /// <summary>
    /// Renders the first page of the registration process where user enters an email.
    /// </summary>
    [HttpGet, AllowAnonymous]
    [FeatureGate(InternalFeatureFlags.UserRegistration)]
    public IActionResult Register() {
        if (IsAuthenticated) {
            return RedirectToDashboard;
        }

        return View(_RegisterView, new RegisterModel());
    }

    /// <summary>
    /// Registers new user if valid. Renders first register page on error or login page.
    /// </summary>
    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    [FeatureGate(InternalFeatureFlags.UserRegistration)]
    public async Task<IActionResult> Register(RegisterModel model, CancellationToken cancellationToken = default) {
        if (IsAuthenticated) {
            return RedirectToDashboard;
        }

        if (!ModelState.IsValid) {
            return ViewWithError(_RegisterView, model, ModelState);
        }

        var defaultRole = await _RoleService.GetDefaultRoleAsync(cancellationToken);
        if (defaultRole == null) {
            return ViewWithError(_RegisterView, model, Core.ErrorGeneric);
        }

        var newUser = new UserModel {
            Email = model.Email, Name = model.Name, LanguageId = model.LanguageId,
            RoleIds = [defaultRole.Id]
        };
        (var success, var message) = await _UserService.SaveUserAsync(newUser, cancellationToken);
        if (!success) {
            return ViewWithError(_RegisterView, model, message);
        }

        return ViewWithMessage(_LoginView, new LoginModel { Email = model.Email }, Account.UserRegistered);
    }

    /// <summary>
    /// Renders login page.
    /// </summary>
    [HttpGet, AllowAnonymous]
    public IActionResult Login(LoginModel model) {
        if (IsAuthenticated) {
            return RedirectToDashboard;
        }

        return View(_LoginView, model);
    }

    /// <summary>
    /// Create login token if valid. Renders login page on error or token page.
    /// </summary>
    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginModel model, CancellationToken cancellationToken) {
        if (IsAuthenticated) {
            return RedirectToDashboard;
        }

        if (!ModelState.IsValid) {
            return ViewWithError(_LoginView, model, ModelState);
        }

        var user = await _UserService.GetUserByEmailAsync(model.Email!, cancellationToken);
        if (user == null) {
            return ViewWithError(_LoginView, model, Account.ErrorInvalidUser);
        }

        (var success, var token, var secondaryToken) = await _UserService.CreateUserTokenAsync(user.Id, cancellationToken);
        if (!success || string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(secondaryToken)) {
            return ViewWithError(_LoginView, model, Account.ErrorSendingToken);
        }

        // send token in email
        var emailBody = await _ViewService.RenderViewToStringAsync(_TokenEmailView,
            new LoginTokenModel { Email = model.Email, Token = token, SecondaryToken = secondaryToken }, ControllerContext, true);
        if (!await _EmailService.SendEmailAsync(user.Email, user.Name, Account.TokenEmailSubject, emailBody, cancellationToken)) {
            return ViewWithError(_LoginView, model, Account.ErrorSendingToken);
        }

        // validate returnUrl is local only
        var returnUrl = string.IsNullOrWhiteSpace(model.ReturnUrl) ? null : Url.ConvertToAbsolute(model.ReturnUrl);
        if (!Url.IsLocalUrl(returnUrl)) {
            returnUrl = null;
        }

        return ViewWithMessage(_LoginTokenView,
            new LoginTokenModel { Email = model.Email, SecondaryToken = secondaryToken, ReturnUrl = returnUrl },
            Account.CheckYourEmail
        );
    }

    /// <summary>
    /// Renders token page with default values from an emailed link.
    /// </summary>
    [HttpGet, AllowAnonymous]
    public async Task<IActionResult> LoginByLink(LoginTokenModel model, CancellationToken cancellationToken = default) {
        if (IsAuthenticated) {
            return RedirectToDashboard;
        }

        if (!ModelState.IsValid) {
            return ViewWithError(_LoginView, model, ModelState);
        }

        var user = await _UserService.GetUserByEmailAsync(model.Email, cancellationToken);
        if (user == null) {
            return ViewWithError(_LoginView, model, Account.ErrorInvalidUser);
        }

        return View(_LoginTokenView, new LoginTokenModel { Email = model.Email, Token = model.Token, SecondaryToken = model.SecondaryToken });
    }

    /// <summary>
    /// Logs user in if valid. Renders token or login page on error, or redirects to dashboard on login.
    /// </summary>
    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> LoginToken(LoginTokenModel model, CancellationToken cancellationToken = default) {
        if (IsAuthenticated) {
            return RedirectToDashboard;
        }

        if (!ModelState.IsValid) {
            return ViewWithError(_LoginTokenView, model, ModelState);
        }

        (var success, var message) = await _UserService.VerifyUserTokenAsync(model.Email, model.Token, model.SecondaryToken, cancellationToken);
        if (!success) {
            // if token has been deleted send user to start of login process, else let user try again
            if (message == Account.ErrorTokenDeleted) {
                return ViewWithError(_LoginView, new LoginModel { Email = model.Email }, message);
            }
            return ViewWithError(_LoginTokenView, new LoginTokenModel { Email = model.Email, SecondaryToken = model.SecondaryToken }, message);
        }

        // token valid, load the user info
        var user = await _UserService.GetUserByEmailAsync(model.Email, cancellationToken);
        if (user == null) {
            return ViewWithError(_LoginView, new LoginModel { Email = model.Email }, Account.ErrorTokenDeleted);
        }

        // sign in user and set preferred language
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(await CreateClaimsIdentityForUserAsync(user, cancellationToken)));
        await AddLanguageCookieAsync(user, cancellationToken);

        if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl)) {
            return LocalRedirect(model.ReturnUrl);
        }
        return RedirectToDashboard;
    }

    /// <summary>
    /// Adds name and role claims for the identity.
    /// </summary>
    private async Task<ClaimsIdentity> CreateClaimsIdentityForUserAsync(UserModel user, CancellationToken cancellationToken = default) {
        var claims = new List<Claim> {
            new(ClaimTypes.Name, user.Email),
            new(Auth.UserIdClaim, user.Id.ToString())
        };
        var userClaims = await _UserService.GetClaimsByUserIdAsync(user.Id, cancellationToken);
        if (userClaims.Any()) {
            claims.AddRange(userClaims);
        }

        return new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    }

    /// <summary>
    /// Adds culture cookie to the response to set the user's language.
    /// </summary>
    private async Task AddLanguageCookieAsync(UserModel user, CancellationToken cancellationToken = default) {
        var languageCode = (await _LanguageService.GetLanguageByIdAsync(user.LanguageId, cancellationToken))?.LanguageCode ?? "en";
        HttpContext.Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(new CultureInfo(languageCode))));
    }
    #endregion

    #region Authorized Endpoints
    /// <summary>
    /// Renders the account landing page.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> representing the account landing page.</returns>
    [HttpGet]
    public IActionResult Index() => View(_IndexView);

    /// <summary>
    /// Toggles the visibility of context-sensitive help for the current user session.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> representing the view associated with the updated context help state.</returns>
    [HttpGet, ParentAction(nameof(Index))]
    public IActionResult ToggleContextHelp() {
        HttpContext.Session.ToggleSetting(SessionProperties.Help);
        return View(_ToggleContextHelpView);
    }

    /// <summary>
    /// Renders the update account page.
    /// </summary>
    /// <returns>Account view with the user's current information for updating.</returns>
    [HttpGet, ParentAction(nameof(Index))]
    public async Task<IActionResult> UpdateAccount(CancellationToken cancellationToken = default) {
        var user = await _UserService.GetUserByEmailAsync(User.Identity!.Name!, cancellationToken);
        if (user == null) {
            return ViewWithError(_IndexView, null, Core.ErrorInvalidId);
        }

        return View(_UpdateAccountView, new UpdateAccountModel(user));
    }

    /// <summary>
    /// Save account changes and renders the update account page.
    /// </summary>
    /// <returns>Account view with success or error message.</returns>
    [HttpPost, ParentAction(nameof(Index))]
    public async Task<IActionResult> UpdateAccount(UpdateAccountModel model, CancellationToken cancellationToken = default) {
        if (!ModelState.IsValid) {
            return ViewWithError(_UpdateAccountView, model, ModelState);
        }

        (var success, var message) = await _UserService.UpdateAccountAsync(model, cancellationToken);
        if (success) {
            var language = await _LanguageService.GetLanguageByIdAsync(model.LanguageId, cancellationToken);
            if (language != null) {
                Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName,
                    CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(new CultureInfo(language.LanguageCode)))
                );
            }
            ViewData.AddMessage(message);
        } else {
            ViewData.AddError(message);
        }
        return View(_UpdateAccountView, model);
    }

    /// <summary>
    /// Log out user and redirect back to login page.
    /// </summary>
    /// <returns>Redirects to the login page after signing out the user.</returns>
    [HttpGet, ParentAction(nameof(Index))]
    public async Task<IActionResult> Logout() {
        // Clear the existing cookie
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }
    #endregion
}
