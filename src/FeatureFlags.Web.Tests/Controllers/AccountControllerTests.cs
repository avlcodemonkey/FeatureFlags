using System.Security.Claims;
using FeatureFlags.Constants;
using FeatureFlags.Controllers;
using FeatureFlags.Extensions;
using FeatureFlags.Models;
using FeatureFlags.Resources;
using FeatureFlags.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;

namespace FeatureFlags.Web.Tests.Controllers;

public class AccountControllerTests {
    private readonly Mock<IUserService> _MockUserService = new();
    private readonly Mock<ILanguageService> _MockLanguageService = new();
    private readonly Mock<IEmailService> _MockEmailService = new();
    private readonly Mock<IViewService> _MockViewService = new();
    private readonly Mock<IRoleService> _MockRoleService = new();
    private readonly Mock<ILogger<AccountController>> _MockLogger = new();

    private readonly UserModel _UserForSuccess = new() { Id = -100, Email = "a@b.com" };
    private readonly UserModel _UserForFailure = new() { Id = -200, Email = "b@c.com" };
    private readonly UserModel _UserForLogin = new() { Id = -300, Email = "c@d.com" };

    private readonly UpdateAccountModel _UpdateAccountForSucccess = new() { Name = "success", Email = "a@b.com" };
    private readonly UpdateAccountModel _UpdateAccountForFailure = new() { Name = "failure", Email = "b@c.com" };
    private readonly UpdateAccountModel _UpdateAccountForDuplicate = new() { Name = "duplicate", Email = "e@f.com" };

    private readonly LanguageModel _Language = new() { Id = 1, Name = "test lang", LanguageCode = "en" };
    private readonly RoleModel _DefaultRole = new() { Id = 1, Name = "Default", IsDefault = true };

    private readonly string _Url = "/Account/AccessDenied";
    private readonly string _Token = "ABCDEFGH";
    private readonly string _SecondaryToken = "SecondaryToken";
    private readonly string _InvalidToken = "invalid1";
    private readonly string _InvalidSecondaryToken = "invalidSecondary1";
    private readonly string _DefaultReturnUrl = "https://example.com";

    public AccountControllerTests() {
        _MockRoleService.Setup(x => x.GetDefaultRoleAsync(It.IsAny<CancellationToken>())).ReturnsAsync(_DefaultRole);

        _MockUserService.Setup(x => x.SaveUserAsync(It.IsAny<UserModel>(), It.IsAny<CancellationToken>())).ReturnsAsync((true, Account.UserRegistered));
        _MockUserService.Setup(x => x.GetUserByEmailAsync(_UserForLogin.Email, It.IsAny<CancellationToken>())).ReturnsAsync(_UserForLogin);
        _MockUserService.Setup(x => x.GetUserByEmailAsync(_UserForFailure.Email, It.IsAny<CancellationToken>())).ReturnsAsync(null as UserModel);
        _MockUserService.Setup(x => x.CreateUserTokenAsync(_UserForLogin.Id, It.IsAny<CancellationToken>())).ReturnsAsync((true, _Token, _SecondaryToken));
        _MockUserService.Setup(x => x.VerifyUserTokenAsync(_UserForLogin.Email, _Token, _SecondaryToken, It.IsAny<CancellationToken>())).ReturnsAsync((true, ""));
        _MockUserService.Setup(x => x.VerifyUserTokenAsync(_UserForFailure.Email, _Token, _SecondaryToken, It.IsAny<CancellationToken>())).ReturnsAsync((true, ""));
        _MockUserService.Setup(x => x.VerifyUserTokenAsync(_UserForFailure.Email, _InvalidToken, _InvalidSecondaryToken, It.IsAny<CancellationToken>())).ReturnsAsync((false, Core.ErrorGeneric));

        _MockEmailService.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    }

    private AccountController CreateController(string? userName = null, bool isAnonymous = false) {
        var authClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, userName ?? _UserForSuccess.Email)],
            CookieAuthenticationDefaults.AuthenticationScheme));

        var mockUrlHelper = new Mock<IUrlHelper>();
        mockUrlHelper.Setup(x => x.Action(It.IsAny<UrlActionContext>())).Returns(_Url);
        mockUrlHelper.Setup(x => x.IsLocalUrl(_DefaultReturnUrl)).Returns(true);

        var mockAuthService = new Mock<IAuthenticationService>();
        mockAuthService.Setup(x => x.SignInAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthenticationProperties>()))
            .Returns(Task.FromResult((object)null!));

        var mockAuthTempData = new Mock<ITempDataDictionaryFactory>();

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(x => x.GetService(typeof(IAuthenticationService))).Returns(mockAuthService.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(ITempDataDictionaryFactory))).Returns(mockAuthTempData.Object);

        return new AccountController(_MockUserService.Object, _MockLanguageService.Object, _MockEmailService.Object, _MockViewService.Object, _MockRoleService.Object, _MockLogger.Object) {
            ControllerContext = new ControllerContext {
                HttpContext = new DefaultHttpContext() {
                    Session = new Mock<ISession>().Object,
                    User = isAnonymous ? new ClaimsPrincipal() : authClaimsPrincipal,
                    RequestServices = serviceProviderMock.Object,
                }
            },
            Url = mockUrlHelper.Object,
        };
    }

    #region Anonymous Endpoints
    [Fact]
    public void Get_RegisterWithAuthenticatedUser_RedirectsToDashboard() {
        // Arrange
        var controller = CreateController();

        // Act
        var result = controller.Register();

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(DashboardController).StripController(), redirectResult.ControllerName);
        Assert.Equal(nameof(DashboardController.Index), redirectResult.ActionName);
    }

    [Fact]
    public void Get_RegisterWithAnonymousUser_ReturnsRegisterView() {
        // Arrange
        var controller = CreateController(isAnonymous: true);

        // Act
        var result = controller.Register();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Register", viewResult.ViewName);
        var model = Assert.IsType<RegisterModel>(viewResult.ViewData.Model, exactMatch: false);
        Assert.Equal("", model.Email);
        Assert.Equal("", model.Name);
        Assert.Equal(0, model.LanguageId);
    }

    [Fact]
    public async Task Post_RegisterWithAuthenticatedUser_RedirectsToDashboard() {
        // Arrange
        var controller = CreateController();
        var registerModel = new RegisterModel { Email = "a@b.com", Name = "name", LanguageId = 1 };

        // Act
        var result = await controller.Register(registerModel);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(DashboardController).StripController(), redirectResult.ControllerName);
        Assert.Equal(nameof(DashboardController.Index), redirectResult.ActionName);
    }

    [Fact]
    public async Task Post_RegisterWithValidModel_RedirectsToLogin() {
        // Arrange
        var model = new RegisterModel { Email = "test@test.com", Name = "Test User", LanguageId = 1 };
        var controller = CreateController(isAnonymous: true);

        // Act
        var result = await controller.Register(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Login", viewResult.ViewName);
        var viewModel = Assert.IsType<LoginModel>(viewResult.ViewData.Model, exactMatch: false);
        Assert.Equal(model.Email, viewModel.Email);
        Assert.Contains(Account.UserRegistered, viewResult.ViewData[ViewProperties.Message]?.ToString());

        _MockUserService.Verify(x => x.SaveUserAsync(It.Is<UserModel>(
                x => x.Email == model.Email && x.Name == model.Name && x.LanguageId == model.LanguageId && x.RoleIds!.Contains(_DefaultRole.Id)
            ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Post_RegisterWithInvalidModel_ReturnViewWithError() {
        // Arrange
        var model = new RegisterModel { Email = "test@test.com", Name = "Test User", LanguageId = 1 };
        var controller = CreateController(isAnonymous: true);
        controller.ModelState.AddModelError("", "test");

        // Act
        var result = await controller.Register(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Register", viewResult.ViewName);
        var viewModel = Assert.IsType<RegisterModel>(viewResult.ViewData.Model, exactMatch: false);
        Assert.Equal(model.Email, viewModel.Email);
        Assert.Contains("test", viewResult.ViewData[ViewProperties.Error]?.ToString());
    }

    [Fact]
    public async Task Post_RegisterWithNoDefaultRole_ReturnViewWithError() {
        // Arrange
        var model = new RegisterModel { Email = "test@test.com", Name = "Test User", LanguageId = 1 };
        var mockRoleService = new Mock<IRoleService>();
        mockRoleService.Setup(x => x.GetDefaultRoleAsync(It.IsAny<CancellationToken>())).ReturnsAsync(null as RoleModel);
        var controller = new AccountController(_MockUserService.Object, _MockLanguageService.Object, _MockEmailService.Object, _MockViewService.Object, mockRoleService.Object, _MockLogger.Object) {
            ControllerContext = new ControllerContext {
                HttpContext = new DefaultHttpContext() {
                    Session = new Mock<ISession>().Object,
                    User = new ClaimsPrincipal()
                }
            }
        };

        // Act
        var result = await controller.Register(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Register", viewResult.ViewName);
        var viewModel = Assert.IsType<RegisterModel>(viewResult.ViewData.Model, exactMatch: false);
        Assert.Equal(model.Email, viewModel.Email);
        Assert.Contains(Core.ErrorGeneric, viewResult.ViewData[ViewProperties.Error]?.ToString());
    }

    [Fact]
    public async Task Post_RegisterWithServiceError_ReturnViewWithError() {
        // Arrange
        var model = new RegisterModel { Email = "test@test.com", Name = "Test User", LanguageId = 1 };
        var mockUserService = new Mock<IUserService>();
        mockUserService.Setup(x => x.SaveUserAsync(It.IsAny<UserModel>(), It.IsAny<CancellationToken>())).ReturnsAsync((false, Core.ErrorGeneric));
        var controller = new AccountController(mockUserService.Object, _MockLanguageService.Object, _MockEmailService.Object, _MockViewService.Object, _MockRoleService.Object, _MockLogger.Object) {
            ControllerContext = new ControllerContext {
                HttpContext = new DefaultHttpContext() {
                    Session = new Mock<ISession>().Object,
                    User = new ClaimsPrincipal()
                }
            }
        };

        // Act
        var result = await controller.Register(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Register", viewResult.ViewName);
        var viewModel = Assert.IsType<RegisterModel>(viewResult.ViewData.Model, exactMatch: false);
        Assert.Equal(model.Email, viewModel.Email);
        Assert.Contains(Core.ErrorGeneric, viewResult.ViewData[ViewProperties.Error]?.ToString());
    }

    [Fact]
    public void Get_LoginWithAuthenticatedUser_RedirectsToDashboard() {
        // Arrange
        var controller = CreateController();
        var loginModel = new LoginModel { Email = "a@b.com", ReturnUrl = _DefaultReturnUrl };

        // Act
        var result = controller.Login(loginModel);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(DashboardController).StripController(), redirectResult.ControllerName);
        Assert.Equal(nameof(DashboardController.Index), redirectResult.ActionName);
    }

    [Fact]
    public void Get_LoginWithAnonymousUser_ReturnsLoginView() {
        // Arrange
        var controller = CreateController(isAnonymous: true);
        var loginModel = new LoginModel { Email = "a@b.com", ReturnUrl = _DefaultReturnUrl };

        // Act
        var result = controller.Login(loginModel);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Login", viewResult.ViewName);
        var model = Assert.IsType<LoginModel>(viewResult.ViewData.Model, exactMatch: false);
        Assert.Equal(loginModel.Email, model.Email);
        Assert.Equal(loginModel.ReturnUrl, model.ReturnUrl);
    }

    [Fact]
    public async Task Post_LoginWithAuthenticatedUser_RedirectsToDashboard() {
        // Arrange
        var controller = CreateController();
        var loginModel = new LoginModel { Email = "a@b.com", ReturnUrl = _DefaultReturnUrl };

        // Act
        var result = await controller.Login(loginModel, CancellationToken.None);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(DashboardController).StripController(), redirectResult.ControllerName);
        Assert.Equal(nameof(DashboardController.Index), redirectResult.ActionName);
    }

    [Fact]
    public async Task Post_LoginWithValidModel_RedirectsToLoginToken() {
        // Arrange
        var model = new LoginModel { Email = _UserForLogin.Email, ReturnUrl = _DefaultReturnUrl };
        var controller = CreateController(isAnonymous: true);

        // Act
        var result = await controller.Login(model, CancellationToken.None);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("LoginToken", viewResult.ViewName);
        var viewModel = Assert.IsType<LoginTokenModel>(viewResult.ViewData.Model, exactMatch: false);
        Assert.Equal(model.Email, viewModel.Email);
        Assert.Equal(model.ReturnUrl, viewModel.ReturnUrl);
        Assert.Contains(Account.CheckYourEmail, viewResult.ViewData[ViewProperties.Message]?.ToString());

        _MockViewService.Verify(x => x.RenderViewToStringAsync("TokenEmail", It.Is<LoginTokenModel>(y => y.Email == model.Email && y.Token == _Token),
            It.IsAny<ControllerContext>(), It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task Post_LoginWithValidModelAndInvalidReturnUrl_RedirectsToLoginTokenWithEmptyReturnUrl() {
        // Arrange
        var model = new LoginModel { Email = _UserForLogin.Email, ReturnUrl = "https://another-domain.com/test" };
        var controller = CreateController(isAnonymous: true);

        // Act
        var result = await controller.Login(model, CancellationToken.None);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("LoginToken", viewResult.ViewName);
        var viewModel = Assert.IsType<LoginTokenModel>(viewResult.ViewData.Model, exactMatch: false);
        Assert.Equal(model.Email, viewModel.Email);
        Assert.Null(viewModel.ReturnUrl);
        Assert.Contains(Account.CheckYourEmail, viewResult.ViewData[ViewProperties.Message]?.ToString());

        _MockViewService.Verify(x => x.RenderViewToStringAsync("TokenEmail", It.Is<LoginTokenModel>(y => y.Email == model.Email && y.Token == _Token),
            It.IsAny<ControllerContext>(), It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task Post_LoginWithInvalidModel_ReturnViewWithError() {
        // Arrange
        var model = new LoginModel { Email = _UserForLogin.Email, ReturnUrl = _DefaultReturnUrl };
        var controller = CreateController(isAnonymous: true);
        controller.ModelState.AddModelError("", "test");

        // Act
        var result = await controller.Login(model, CancellationToken.None);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Login", viewResult.ViewName);
        var viewModel = Assert.IsType<LoginModel>(viewResult.ViewData.Model, exactMatch: false);
        Assert.Equal(model.Email, viewModel.Email);
        Assert.Equal(model.ReturnUrl, viewModel.ReturnUrl);
        Assert.Contains("test", viewResult.ViewData[ViewProperties.Error]?.ToString());
    }

    [Fact]
    public async Task Post_LoginWithInvalidUser_ReturnViewWithError() {
        // Arrange
        var model = new LoginModel { Email = "invalidLogin@test.com", ReturnUrl = _DefaultReturnUrl };
        var mockRoleService = new Mock<IRoleService>();
        mockRoleService.Setup(x => x.GetDefaultRoleAsync(It.IsAny<CancellationToken>())).ReturnsAsync(null as RoleModel);
        var controller = new AccountController(_MockUserService.Object, _MockLanguageService.Object, _MockEmailService.Object, _MockViewService.Object, mockRoleService.Object, _MockLogger.Object) {
            ControllerContext = new ControllerContext {
                HttpContext = new DefaultHttpContext() {
                    Session = new Mock<ISession>().Object,
                    User = new ClaimsPrincipal()
                }
            }
        };

        // Act
        var result = await controller.Login(model, CancellationToken.None);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Login", viewResult.ViewName);
        var viewModel = Assert.IsType<LoginModel>(viewResult.ViewData.Model, exactMatch: false);
        Assert.Equal(model.Email, viewModel.Email);
        Assert.Equal(model.ReturnUrl, viewModel.ReturnUrl);
        Assert.Contains(Account.ErrorInvalidUser, viewResult.ViewData[ViewProperties.Error]?.ToString());
    }

    [Fact]
    public async Task Post_LoginWithUserServiceError_ReturnViewWithError() {
        // Arrange
        var model = new LoginModel { Email = _UserForLogin.Email, ReturnUrl = _DefaultReturnUrl };
        var mockUserService = new Mock<IUserService>();
        mockUserService.Setup(x => x.GetUserByEmailAsync(_UserForLogin.Email, It.IsAny<CancellationToken>())).ReturnsAsync(_UserForLogin);
        mockUserService.Setup(x => x.CreateUserTokenAsync(_UserForLogin.Id, It.IsAny<CancellationToken>())).ReturnsAsync((false, "", ""));
        var controller = new AccountController(mockUserService.Object, _MockLanguageService.Object, _MockEmailService.Object, _MockViewService.Object, _MockRoleService.Object, _MockLogger.Object) {
            ControllerContext = new ControllerContext {
                HttpContext = new DefaultHttpContext() {
                    Session = new Mock<ISession>().Object,
                    User = new ClaimsPrincipal()
                }
            }
        };

        // Act
        var result = await controller.Login(model, CancellationToken.None);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Login", viewResult.ViewName);
        var viewModel = Assert.IsType<LoginModel>(viewResult.ViewData.Model, exactMatch: false);
        Assert.Equal(model.Email, viewModel.Email);
        Assert.Equal(model.ReturnUrl, viewModel.ReturnUrl);
        Assert.Contains(Account.ErrorSendingToken, viewResult.ViewData[ViewProperties.Error]?.ToString());

        mockUserService.Verify(x => x.CreateUserTokenAsync(_UserForLogin.Id, It.IsAny<CancellationToken>()), Times.Once);
        _MockViewService.Verify(x => x.RenderViewToStringAsync("TokenEmail", It.Is<LoginTokenModel>(y => y.Email == model.Email && y.Token == _Token),
            It.IsAny<ControllerContext>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task Post_LoginWithEmailServiceError_ReturnViewWithError() {
        // Arrange
        var model = new LoginModel { Email = _UserForLogin.Email, ReturnUrl = _DefaultReturnUrl };
        var mockEmailService = new Mock<IEmailService>();
        mockEmailService.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var controller = new AccountController(_MockUserService.Object, _MockLanguageService.Object, mockEmailService.Object, _MockViewService.Object, _MockRoleService.Object, _MockLogger.Object) {
            ControllerContext = new ControllerContext {
                HttpContext = new DefaultHttpContext() {
                    Session = new Mock<ISession>().Object,
                    User = new ClaimsPrincipal()
                }
            }
        };

        // Act
        var result = await controller.Login(model, CancellationToken.None);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Login", viewResult.ViewName);
        var viewModel = Assert.IsType<LoginModel>(viewResult.ViewData.Model, exactMatch: false);
        Assert.Equal(model.Email, viewModel.Email);
        Assert.Equal(model.ReturnUrl, viewModel.ReturnUrl);
        Assert.Contains(Account.ErrorSendingToken, viewResult.ViewData[ViewProperties.Error]?.ToString());

        _MockUserService.Verify(x => x.CreateUserTokenAsync(_UserForLogin.Id, It.IsAny<CancellationToken>()), Times.Once);
        _MockViewService.Verify(x => x.RenderViewToStringAsync("TokenEmail", It.Is<LoginTokenModel>(y => y.Email == model.Email && y.Token == _Token),
            It.IsAny<ControllerContext>(), It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task Get_LoginByLinkWithAuthenticatedUser_RedirectsToDashboard() {
        // Arrange
        var controller = CreateController();
        var loginTokenModel = new LoginTokenModel { Email = _UserForLogin.Email, Token = "token", ReturnUrl = _DefaultReturnUrl };

        // Act
        var result = await controller.LoginByLink(loginTokenModel);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(DashboardController).StripController(), redirectResult.ControllerName);
        Assert.Equal(nameof(DashboardController.Index), redirectResult.ActionName);
    }

    [Fact]
    public async Task Get_LoginByLinkWithAnonymousUser_ReturnsLoginView() {
        // Arrange
        _MockUserService.Setup(x => x.GetUserByEmailAsync(_UserForLogin.Email, It.IsAny<CancellationToken>())).ReturnsAsync(_UserForLogin);
        var controller = CreateController(isAnonymous: true);
        var loginTokenModel = new LoginTokenModel { Email = _UserForLogin.Email, Token = "token" };

        // Act
        var result = await controller.LoginByLink(loginTokenModel);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("LoginToken", viewResult.ViewName);
        var model = Assert.IsType<LoginTokenModel>(viewResult.ViewData.Model, exactMatch: false);
        Assert.Equal(loginTokenModel.Email, model.Email);
        Assert.Equal(loginTokenModel.Token, model.Token);
        Assert.Null(model.ReturnUrl);
    }

    [Fact]
    public async Task Post_LoginTokenWithAuthenticatedUser_RedirectsToDashboard() {
        // Arrange
        var controller = CreateController();
        var loginTokenModel = new LoginTokenModel { Email = _UserForLogin.Email, Token = "token", ReturnUrl = _DefaultReturnUrl };

        // Act
        var result = await controller.LoginToken(loginTokenModel);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(DashboardController).StripController(), redirectResult.ControllerName);
        Assert.Equal(nameof(DashboardController.Index), redirectResult.ActionName);
    }

    [Fact]
    public async Task Post_LoginTokenWithValidModelAndNoReturnUrl_RedirectsToDashboard() {
        // Arrange
        var loginTokenModel = new LoginTokenModel { Email = _UserForLogin.Email, Token = _Token, SecondaryToken = _SecondaryToken };
        var controller = CreateController(isAnonymous: true);

        // Act
        var result = await controller.LoginToken(loginTokenModel);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(DashboardController).StripController(), redirectResult.ControllerName);
        Assert.Equal(nameof(DashboardController.Index), redirectResult.ActionName);
    }

    [Fact]
    public async Task Post_LoginTokenWithValidModelAndReturnUrl_RedirectsToReturnUrl() {
        // Arrange
        var loginTokenModel = new LoginTokenModel {
            Email = _UserForLogin.Email, Token = _Token,
            SecondaryToken = _SecondaryToken, ReturnUrl = _DefaultReturnUrl
        };
        var controller = CreateController(isAnonymous: true);

        // Act
        var result = await controller.LoginToken(loginTokenModel);

        // Assert
        var redirectResult = Assert.IsType<LocalRedirectResult>(result);
        Assert.Equal(_DefaultReturnUrl, redirectResult.Url);
    }

    [Fact]
    public async Task Post_LoginTokenWithValidModelAndInvalidReturnUrl_RedirectsToDashboard() {
        // Arrange
        var loginTokenModel = new LoginTokenModel {
            Email = _UserForLogin.Email, Token = _Token,
            SecondaryToken = _SecondaryToken, ReturnUrl = "https://anotherdomain.com/test"
        };
        var controller = CreateController(isAnonymous: true);

        // Act
        var result = await controller.LoginToken(loginTokenModel);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(DashboardController).StripController(), redirectResult.ControllerName);
        Assert.Equal(nameof(DashboardController.Index), redirectResult.ActionName);
    }

    [Fact]
    public async Task Post_LoginTokenWithInvalidModel_ReturnViewWithError() {
        // Arrange
        var loginTokenModel = new LoginTokenModel { Email = _UserForLogin.Email, Token = "token", ReturnUrl = _DefaultReturnUrl };
        var controller = CreateController(isAnonymous: true);
        controller.ModelState.AddModelError("", "test");

        // Act
        var result = await controller.LoginToken(loginTokenModel);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("LoginToken", viewResult.ViewName);
        var viewModel = Assert.IsType<LoginTokenModel>(viewResult.ViewData.Model, exactMatch: false);
        Assert.Equal(loginTokenModel.Email, viewModel.Email);
        Assert.Equal(loginTokenModel.Token, viewModel.Token);
        Assert.Equal(loginTokenModel.ReturnUrl, viewModel.ReturnUrl);
        Assert.Contains("test", viewResult.ViewData[ViewProperties.Error]?.ToString());
    }

    [Fact]
    public async Task Post_LoginTokenWithInvalidToken_ReturnViewWithError() {
        // Arrange
        var loginTokenModel = new LoginTokenModel {
            Email = _UserForFailure.Email, Token = _InvalidToken,
            SecondaryToken = _InvalidSecondaryToken, ReturnUrl = _DefaultReturnUrl
        };
        var controller = CreateController(isAnonymous: true);

        // Act
        var result = await controller.LoginToken(loginTokenModel);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("LoginToken", viewResult.ViewName);
        var viewModel = Assert.IsType<LoginTokenModel>(viewResult.ViewData.Model, exactMatch: false);
        Assert.Equal(loginTokenModel.Email, viewModel.Email);
        Assert.Contains(Core.ErrorGeneric, viewResult.ViewData[ViewProperties.Error]?.ToString());
    }

    [Fact]
    public async Task Post_LoginTokenWithUserServiceError_ReturnViewWithError() {
        // Arrange
        var loginTokenModel = new LoginTokenModel {
            Email = _UserForFailure.Email, Token = _Token,
            SecondaryToken = _SecondaryToken, ReturnUrl = _DefaultReturnUrl
        };
        var controller = CreateController(isAnonymous: true);

        // Act
        var result = await controller.LoginToken(loginTokenModel);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Login", viewResult.ViewName);
        var viewModel = Assert.IsType<LoginModel>(viewResult.ViewData.Model, exactMatch: false);
        Assert.Equal(loginTokenModel.Email, viewModel.Email);
        Assert.Contains(Account.ErrorTokenDeleted, viewResult.ViewData[ViewProperties.Error]?.ToString());

        _MockUserService.Verify(x => x.GetUserByEmailAsync(_UserForFailure.Email, It.IsAny<CancellationToken>()), Times.Once);
    }
    #endregion

    #region Authorized Endpoints
    [Fact]
    public void Get_ToggleContextHelp_EnablesHelp() {
        // Arrange
        var controller = CreateController();

        // Act
        var result = controller.ToggleContextHelp();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("ToggleContextHelp", viewResult.ViewName);
        Assert.Null(viewResult.ViewData.Model);
    }

    [Fact]
    public async Task Get_UpdateAccount_WithValidUser_ReturnsView() {
        // Arrange
        _MockUserService.Setup(x => x.GetUserByEmailAsync(_UserForSuccess.Email, It.IsAny<CancellationToken>())).ReturnsAsync(_UserForSuccess);
        var controller = CreateController();

        // Act
        var result = await controller.UpdateAccount();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("UpdateAccount", viewResult.ViewName);

        var model = Assert.IsType<UpdateAccountModel>(viewResult.ViewData.Model, exactMatch: false);
        Assert.Equal(_UserForSuccess.Email, model.Email);

        _MockUserService.Verify(x => x.GetUserByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Get_UpdateAccount_WithInvalidUser_ReturnsError() {
        // Arrange
        var controller = CreateController(_UserForFailure.Email);

        // Act
        var result = await controller.UpdateAccount();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", viewResult.ViewName);
        Assert.Equal(Core.ErrorInvalidId, viewResult.ViewData[ViewProperties.Error]?.ToString());

        _MockUserService.Verify(x => x.GetUserByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Post_UpdateAccount_WithInvalidModel_ReturnsUpdateWithError() {
        // Arrange
        var controller = CreateController();
        controller.ModelState.AddModelError("error", "some error");

        // Act
        var result = await controller.UpdateAccount(_UpdateAccountForSucccess);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("UpdateAccount", viewResult.ViewName);
        Assert.Contains("some error", viewResult.ViewData[ViewProperties.Error]?.ToString());

        var model = Assert.IsType<UpdateAccountModel>(viewResult.ViewData.Model, exactMatch: false);
        Assert.Equal(_UpdateAccountForSucccess.Email, model.Email);

        _MockUserService.Verify(x => x.UpdateAccountAsync(It.IsAny<UpdateAccountModel>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Post_UpdateAccount_WithValidModel_UpdatesAccount() {
        // Arrange
        _MockUserService.Setup(x => x.GetUserByEmailAsync(_UserForSuccess.Email, It.IsAny<CancellationToken>())).ReturnsAsync(_UserForSuccess);
        _MockUserService.Setup(x => x.GetUserByEmailAsync(_UserForFailure.Email, It.IsAny<CancellationToken>())).ReturnsAsync(null as UserModel);
        _MockUserService.Setup(x => x.UpdateAccountAsync(_UpdateAccountForSucccess, It.IsAny<CancellationToken>())).ReturnsAsync((true, Account.AccountUpdated));
        _MockLanguageService.Setup(x => x.GetLanguageByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(_Language);
        var controller = CreateController();

        // Act
        var result = await controller.UpdateAccount(_UpdateAccountForSucccess);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("UpdateAccount", viewResult.ViewName);
        Assert.Equal(Account.AccountUpdated, viewResult.ViewData[ViewProperties.Message]?.ToString());

        var model = Assert.IsType<UpdateAccountModel>(viewResult.ViewData.Model, exactMatch: false);
        Assert.Equal(_UpdateAccountForSucccess.Email, model.Email);

        Assert.True(controller.Response.Headers.TryGetValue("Set-Cookie", out var cookieValue));
        Assert.Contains(CookieRequestCultureProvider.DefaultCookieName, cookieValue.ToString());

        _MockUserService.Verify(x => x.UpdateAccountAsync(It.IsAny<UpdateAccountModel>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Post_UpdateAccount_WithUserServiceError_ReturnsError() {
        // Arrange
        _MockUserService.Setup(x => x.GetUserByEmailAsync(_UserForSuccess.Email, It.IsAny<CancellationToken>())).ReturnsAsync(_UserForSuccess);
        _MockUserService.Setup(x => x.UpdateAccountAsync(_UpdateAccountForFailure, It.IsAny<CancellationToken>())).ReturnsAsync((false, Core.ErrorGeneric));
        var controller = CreateController();

        // Act
        var result = await controller.UpdateAccount(_UpdateAccountForFailure);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("UpdateAccount", viewResult.ViewName);
        Assert.Equal(Core.ErrorGeneric, viewResult.ViewData[ViewProperties.Error]?.ToString());

        var model = Assert.IsType<UpdateAccountModel>(viewResult.ViewData.Model, exactMatch: false);
        Assert.Equal(_UpdateAccountForFailure.Email, model.Email);

        Assert.False(controller.Response.Headers.TryGetValue("Set-Cookie", out _));

        _MockUserService.Verify(x => x.UpdateAccountAsync(It.IsAny<UpdateAccountModel>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Post_UpdateAccount_WithDuplicateEmail_ReturnsError() {
        // Arrange
        _MockUserService.Setup(x => x.UpdateAccountAsync(_UpdateAccountForDuplicate, It.IsAny<CancellationToken>())).ReturnsAsync((false, Users.ErrorDuplicateEmail));
        var controller = CreateController();

        // Act
        var result = await controller.UpdateAccount(_UpdateAccountForDuplicate);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("UpdateAccount", viewResult.ViewName);
        Assert.Equal(Users.ErrorDuplicateEmail, viewResult.ViewData[ViewProperties.Error]?.ToString());

        var model = Assert.IsType<UpdateAccountModel>(viewResult.ViewData.Model, exactMatch: false);
        Assert.Equal(_UpdateAccountForDuplicate.Email, model.Email);

        Assert.False(controller.Response.Headers.TryGetValue("Set-Cookie", out _));

        _MockUserService.Verify(x => x.UpdateAccountAsync(It.IsAny<UpdateAccountModel>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    #endregion
}
