using System.Security.Claims;
using FeatureFlags.Constants;
using FeatureFlags.Controllers;
using FeatureFlags.Models;
using FeatureFlags.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;
using Moq;

namespace FeatureFlags.Web.Tests.Controllers;

public class BaseControllerTests() {
    private readonly Mock<IUserService> _MockUserService = new();
    private readonly Mock<ILanguageService> _MockLanguageService = new();
    private readonly Mock<IEmailService> _MockEmailService = new();
    private readonly Mock<IViewService> _MockViewService = new();
    private readonly Mock<IRoleService> _MockRoleService = new();
    private readonly Mock<ILogger<AccountController>> _MockLogger = new();

    private readonly UserModel _User = new() { Id = -100, Email = "a@b.com" };
    private readonly string _Url = "/Account/AccessDenied";

    private AccountController CreateController(string? userName = null) {
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, userName ?? _User.Email)]));

        var mockUrlHelper = new Mock<IUrlHelper>();
        mockUrlHelper.Setup(x => x.Action(It.IsAny<UrlActionContext>())).Returns(_Url);

        // use an instance of AccountController to get access to the base controller methods we want to test
        return new AccountController(_MockUserService.Object, _MockLanguageService.Object, _MockEmailService.Object, _MockViewService.Object, _MockRoleService.Object, _MockLogger.Object) {
            ControllerContext = new ControllerContext {
                HttpContext = new DefaultHttpContext() {
                    Session = new Mock<ISession>().Object,
                    User = claimsPrincipal
                }
            },
            Url = mockUrlHelper.Object,
        };
    }

    [Fact]
    public void AddPushState_WithAction_AddsHeader() {
        // Arrange
        var url = _Url;
        var controller = CreateController();

        // Act
        controller.AddPushState("AccessDenied");

        // Assert
        Assert.True(controller.Response.Headers.TryGetValue(PJax.PushUrl, out var headerUrl));
        Assert.Equal(url, headerUrl.ToString());
    }

    [Fact]
    public void AddPushState_WithEmptyAction_DoesNotAddHeader() {
        // Arrange
        var controller = CreateController();

        // Act
        controller.AddPushState("");

        // Assert
        Assert.False(controller.Response.Headers.TryGetValue(PJax.PushUrl, out _));
    }

    [Fact]
    public void AddPushState_WithNullAction_DoesNotAddHeader() {
        // Arrange
        var controller = CreateController();

        // Act
        controller.AddPushState(null!);

        // Assert
        Assert.False(controller.Response.Headers.TryGetValue(PJax.PushUrl, out _));
    }

    [Fact]
    public void ViewWithMessage_ReturnsViewAndMessage() {
        // Arrange
        var controller = CreateController();
        var testModel = new LoginModel { Email = "a@b.com" };

        // Act
        var result = controller.ViewWithMessage("Test", testModel, "Message");

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Test", viewResult.ViewName);
        Assert.Equal("Message", viewResult.ViewData[ViewProperties.Message]?.ToString());

        var model = Assert.IsType<LoginModel>(viewResult.ViewData.Model, exactMatch: false);
        Assert.Equal(testModel.Email, model.Email);
    }

    [Fact]
    public void ViewWithError_Message_ReturnsViewAndMessage() {
        // Arrange
        var controller = CreateController();
        var testModel = new LoginModel { Email = "a@b.com" };

        // Act
        var result = controller.ViewWithError("Test", testModel, "error");

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Test", viewResult.ViewName);
        Assert.Equal("error", viewResult.ViewData[ViewProperties.Error]?.ToString());

        var model = Assert.IsType<LoginModel>(viewResult.ViewData.Model, exactMatch: false);
        Assert.Equal(testModel.Email, model.Email);
    }

    [Fact]
    public void ViewWithError_ModelState_ReturnsViewAndMessage() {
        // Arrange
        var controller = CreateController();
        var testModel = new LoginModel { Email = "a@b.com" };
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("key", "error");

        // Act
        var result = controller.ViewWithError("Test", testModel, modelState);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Test", viewResult.ViewName);
        Assert.Equal("error", viewResult.ViewData[ViewProperties.Error]?.ToString());

        var model = Assert.IsType<LoginModel>(viewResult.ViewData.Model, exactMatch: false);
        Assert.Equal(testModel.Email, model.Email);
    }
}
