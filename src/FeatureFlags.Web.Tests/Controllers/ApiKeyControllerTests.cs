using System.Security.Claims;
using FeatureFlags.Controllers;
using FeatureFlags.Models;
using FeatureFlags.Resources;
using FeatureFlags.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;
using Moq;

namespace FeatureFlags.Web.Tests.Controllers;

public class ApiKeyControllerTests {
    private readonly Mock<IApiKeyService> _MockApiKeyService = new();
    private readonly Mock<IUserService> _MockUserService = new();
    private readonly Mock<ILogger<ApiKeyController>> _MockLogger = new();

    private readonly string _Url = "/ApiKey/Index";
    private readonly string _DefaultReturnUrl = "https://example.com";

    private ApiKeyController CreateController() {
        var mockUrlHelper = new Mock<IUrlHelper>();
        mockUrlHelper.Setup(x => x.Action(It.IsAny<UrlActionContext>())).Returns(_Url);
        mockUrlHelper.Setup(x => x.IsLocalUrl(_DefaultReturnUrl)).Returns(true);

        var controller = new ApiKeyController(_MockApiKeyService.Object, _MockUserService.Object, _MockLogger.Object) {
            ControllerContext = new ControllerContext {
                HttpContext = new DefaultHttpContext()
            },
            Url = mockUrlHelper.Object,
        };
        return controller;
    }

    [Fact]
    public void Get_Index_ReturnsView() {
        var controller = CreateController();

        var result = controller.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", viewResult.ViewName);
    }

    [Fact]
    public async Task Get_List_ReturnsApiKeyListAsJson() {
        var apiKeys = new[]
        {
            new ApiKeyModel { Id = 1, Name = "Test", Key = "abc", CreatedDate = DateTime.UtcNow }
        };
        _MockApiKeyService.Setup(s => s.GetAllApiKeysAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiKeys);

        var controller = CreateController();

        var result = await controller.List();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var list = Assert.IsType<IEnumerable<ApiKeyListResultModel>>(okResult.Value, exactMatch: false);
        Assert.Single(list);
        Assert.Equal("Test", list.First().Name);
    }

    [Fact]
    public void Get_Create_ReturnsViewWithModel() {
        var controller = CreateController();

        var result = controller.Create();

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Create", viewResult.ViewName);
        Assert.IsType<ApiKeyModel>(viewResult.Model);
        Assert.False(string.IsNullOrWhiteSpace(((ApiKeyModel)viewResult.Model!).Key));
    }

    [Fact]
    public async Task Post_Create_InvalidModel_ReturnsViewWithError() {
        var controller = CreateController();
        controller.ModelState.AddModelError("Name", "Required");

        var model = new ApiKeyModel();

        var result = await controller.Create(model);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Create", viewResult.ViewName);
        Assert.Equal(model, viewResult.Model);
    }

    [Fact]
    public async Task Post_Create_SaveFails_ReturnsViewWithError() {
        _MockApiKeyService.Setup(s => s.SaveApiKeyAsync(It.IsAny<ApiKeyModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, "error"));

        var controller = CreateController();
        var model = new ApiKeyModel { Name = "Test", Key = "abc" };

        var result = await controller.Create(model);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Create", viewResult.ViewName);
        Assert.Equal(model, viewResult.Model);
    }

    [Fact]
    public async Task Post_Create_Success_RedirectsToIndex() {
        _MockApiKeyService.Setup(s => s.SaveApiKeyAsync(It.IsAny<ApiKeyModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, "success"));
        var user = new UserModel { Id = 1, Email = "user@domain.com" };
        _MockUserService.Setup(s => s.GetUserByEmailAsync(user.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var controller = CreateController();
        controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new(ClaimTypes.Name, user.Email)
        }));
        var model = new ApiKeyModel { Name = "Test", Key = "abc" };

        var result = await controller.Create(model);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", viewResult.ViewName);
    }

    [Fact]
    public async Task Delete_InvalidId_ReturnsViewWithError() {
        _MockApiKeyService.Setup(s => s.DeleteApiKeyAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var controller = CreateController();

        var result = await controller.Delete(123);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", viewResult.ViewName);
        Assert.Equal(ApiKeys.ErrorDeletingApiKey, viewResult.ViewData[Constants.ViewProperties.Error]);
    }

    [Fact]
    public async Task Delete_ValidId_ReturnsIndexWithSuccess() {
        _MockApiKeyService.Setup(s => s.DeleteApiKeyAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var controller = CreateController();

        var result = await controller.Delete(123);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", viewResult.ViewName);
        Assert.Equal(ApiKeys.SuccessDeletingApiKey, viewResult.ViewData[Constants.ViewProperties.Message]);
    }

    [Fact]
    public async Task Post_Create_ReturnsError_WhenUserIdentityNameIsNull() {
        // Arrange
        var controller = CreateController();
        controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity()); // No identity name

        var model = new ApiKeyModel { Name = "Test", Key = "abc" };

        // Act
        var result = await controller.Create(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Create", viewResult.ViewName);
        Assert.Equal(model, viewResult.Model);
        Assert.Equal(Core.ErrorGeneric, viewResult.ViewData[Constants.ViewProperties.Error]);
    }

    [Fact]
    public async Task Post_Create_ReturnsError_WhenUserNotFound() {
        // Arrange
        _MockUserService.Setup(s => s.GetUserByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserModel?)null);

        var controller = CreateController();
        controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new(ClaimTypes.Name, "nonexistent@domain.com")
        }));

        var model = new ApiKeyModel { Name = "Test", Key = "abc" };

        // Act
        var result = await controller.Create(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Create", viewResult.ViewName);
        Assert.Equal(model, viewResult.Model);
        Assert.Equal(Core.ErrorGeneric, viewResult.ViewData[Constants.ViewProperties.Error]);
    }

    [Fact]
    public async Task Post_Create_SuccessfullySavesApiKey_WhenUserIdentityNameIsValid() {
        // Arrange
        var user = new UserModel { Id = 1, Email = "user@domain.com" };
        _MockUserService.Setup(s => s.GetUserByEmailAsync(user.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _MockApiKeyService.Setup(s => s.SaveApiKeyAsync(It.IsAny<ApiKeyModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, "success"));

        var controller = CreateController();
        controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new(ClaimTypes.Name, user.Email)
        }));

        var model = new ApiKeyModel { Name = "Test", Key = "abc" };

        // Act
        var result = await controller.Create(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", viewResult.ViewName);
        _MockApiKeyService.Verify(s => s.SaveApiKeyAsync(It.Is<ApiKeyModel>(m => m.UserId == user.Id), It.IsAny<CancellationToken>()), Times.Once);
    }
}
