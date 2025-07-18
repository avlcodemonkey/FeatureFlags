using FeatureFlags.Constants;
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

public class UserControllerTests {
    private readonly Mock<IUserService> _MockUserService = new();
    private readonly Mock<ILogger<UserController>> _MockLogger = new();
    private readonly Mock<IUrlHelper> _MockUrlHelper = new();

    private readonly string _Url = "/test";

    private readonly UserModel _UserForSuccess = new() { Id = -100, Name = "name1", Email = "success@bbb.com" };
    private readonly UserModel _UserForFailure = new() { Id = -101, Name = "name2", Email = "failure@ccc.com" };
    private readonly UserModel _UserForConcurrencyError = new() { Id = -102, Name = "name3", Email = "concurrency@ddd.com" };
    private readonly UserModel _UserForInvalidIdError = new() { Id = -103, Name = "name4", Email = "invalid@email.com" };
    private readonly UserModel _UserForDuplicateEmailError = new() { Id = -104, Name = "name5", Email = "duplicate@eee.com" };

    public UserControllerTests() {
        _MockUserService.Setup(x => x.GetUserByIdAsync(_UserForSuccess.Id, It.IsAny<CancellationToken>())).ReturnsAsync(_UserForSuccess);
        _MockUserService.Setup(x => x.GetAllUsersAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<UserModel> { _UserForSuccess, _UserForFailure });
        _MockUserService.Setup(x => x.GetUserByEmailAsync(_UserForDuplicateEmailError.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_UserForDuplicateEmailError with { Id = -105 });

        _MockUserService.Setup(x => x.SaveUserAsync(_UserForSuccess, It.IsAny<CancellationToken>())).ReturnsAsync((true, Users.SuccessSavingUser));
        _MockUserService.Setup(x => x.SaveUserAsync(_UserForFailure, It.IsAny<CancellationToken>())).ReturnsAsync((false, Core.ErrorGeneric));
        _MockUserService.Setup(x => x.SaveUserAsync(_UserForConcurrencyError, It.IsAny<CancellationToken>())).ReturnsAsync((false, Core.ErrorConcurrency));
        _MockUserService.Setup(x => x.SaveUserAsync(_UserForInvalidIdError, It.IsAny<CancellationToken>())).ReturnsAsync((false, Core.ErrorInvalidId));
        _MockUserService.Setup(x => x.SaveUserAsync(_UserForDuplicateEmailError, It.IsAny<CancellationToken>())).ReturnsAsync((false, Users.ErrorDuplicateEmail));

        _MockUserService.Setup(x => x.DeleteUserAsync(_UserForSuccess.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _MockUserService.Setup(x => x.DeleteUserAsync(_UserForFailure.Id, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        _MockUrlHelper.Setup(x => x.Action(It.IsAny<UrlActionContext>())).Returns(_Url);
    }

    private UserController CreateController() => new(_MockUserService.Object, _MockLogger.Object) {
        ControllerContext = new ControllerContext {
            HttpContext = new DefaultHttpContext()
        },
        Url = _MockUrlHelper.Object
    };

    [Fact]
    public void Get_Index_ReturnsView_WithNoModel() {
        // Arrange
        var controller = CreateController();

        // Act
        var result = controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", viewResult.ViewName);
        Assert.Null(viewResult.ViewData.Model);
    }

    [Fact]
    public async Task Get_List_ReturnsData() {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.List();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<IEnumerable<UserListResultModel>>(okResult.Value, exactMatch: false);
        Assert.Collection(returnValue,
            x => Assert.Equal(_UserForSuccess.Id, x.Id),
            x => Assert.Equal(_UserForFailure.Id, x.Id)
        );
        Assert.Collection(returnValue,
            x => Assert.Equal(_UserForSuccess.Name, x.Name),
            x => Assert.Equal(_UserForFailure.Name, x.Name)
        );
        Assert.Collection(returnValue,
            x => Assert.Equal(_UserForSuccess.Email, x.Email),
            x => Assert.Equal(_UserForFailure.Email, x.Email)
        );
    }

    [Fact]
    public void Get_Create_ReturnsView_WithNewUserModel() {
        // Arrange
        var controller = CreateController();

        // Act
        var result = controller.Create();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("CreateEdit", viewResult.ViewName);

        var model = Assert.IsType<UserModel>(viewResult.ViewData.Model, exactMatch: false);
        Assert.Equal(0, model.Id);
    }

    [Fact]
    public async Task Post_Create_WithValidModel_ReturnsIndexView() {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.Create(_UserForSuccess);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", viewResult.ViewName);
        Assert.Equal(Users.SuccessSavingUser, viewResult.ViewData[ViewProperties.Message]?.ToString());
        Assert.Null(viewResult.ViewData.Model);

        Assert.True(controller.Response.Headers.TryGetValue(PJax.PushUrl, out var headerUrl));
        Assert.Equal(_Url, headerUrl.ToString());

        _MockUserService.Verify(x => x.SaveUserAsync(It.IsAny<UserModel>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Post_Create_WithInvalidModel_ReturnsCreateEditViewWithError() {
        // Arrange
        var controller = CreateController();
        controller.ModelState.AddModelError("error", "some error");

        // Act
        var result = await controller.Create(_UserForSuccess);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("CreateEdit", viewResult.ViewName);
        Assert.Contains("some error", viewResult.ViewData[ViewProperties.Error]?.ToString());

        Assert.False(controller.Response.Headers.TryGetValue(PJax.PushUrl, out var _));

        var model = Assert.IsType<UserModel>(viewResult.ViewData.Model, exactMatch: false);
        Assert.Equal(_UserForSuccess.Id, model.Id);

        _MockUserService.Verify(x => x.SaveUserAsync(It.IsAny<UserModel>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Post_Create_WithServiceError_ReturnsCreateEditViewWithError() {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.Create(_UserForFailure);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("CreateEdit", viewResult.ViewName);
        Assert.Contains(Core.ErrorGeneric, viewResult.ViewData[ViewProperties.Error]?.ToString());

        Assert.False(controller.Response.Headers.TryGetValue(PJax.PushUrl, out var _));

        var model = Assert.IsType<UserModel>(viewResult.ViewData.Model, exactMatch: false);
        Assert.Equal(_UserForFailure.Id, model.Id);

        _MockUserService.Verify(x => x.SaveUserAsync(It.IsAny<UserModel>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Post_Create_WithDuplicateEmail_ReturnsCreateEditViewWithError() {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.Create(_UserForDuplicateEmailError);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("CreateEdit", viewResult.ViewName);
        Assert.Contains(Users.ErrorDuplicateEmail, viewResult.ViewData[ViewProperties.Error]?.ToString());

        Assert.False(controller.Response.Headers.TryGetValue(PJax.PushUrl, out var _));

        var model = Assert.IsType<UserModel>(viewResult.ViewData.Model, exactMatch: false);
        Assert.Equal(_UserForDuplicateEmailError.Id, model.Id);

        _MockUserService.Verify(x => x.SaveUserAsync(It.IsAny<UserModel>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Get_Edit_WithValidId_ReturnsCreateEditViewWithModel() {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.Edit(_UserForSuccess.Id);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("CreateEdit", viewResult.ViewName);

        var model = Assert.IsType<UserModel>(viewResult.ViewData.Model, exactMatch: false);
        Assert.Equal(_UserForSuccess.Id, model.Id);
    }

    [Fact]
    public async Task Get_Edit_WithInvalidId_ReturnsIndexViewWithError() {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.Edit(-200);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", viewResult.ViewName);
        Assert.Equal(Core.ErrorInvalidId, viewResult.ViewData[ViewProperties.Error]?.ToString());
        Assert.Null(viewResult.ViewData.Model);
    }

    [Fact]
    public async Task Post_Edit_WithValidModel_ReturnsIndexView() {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.Edit(_UserForSuccess);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", viewResult.ViewName);
        Assert.Equal(Users.SuccessSavingUser, viewResult.ViewData[ViewProperties.Message]?.ToString());

        Assert.True(controller.Response.Headers.TryGetValue(PJax.PushUrl, out var headerUrl));
        Assert.Equal(_Url, headerUrl.ToString());

        Assert.Null(viewResult.ViewData.Model);

        _MockUserService.Verify(x => x.SaveUserAsync(It.IsAny<UserModel>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Post_Edit_WithInvalidModel_ReturnsCreateEditViewWithError() {
        // Arrange
        var controller = CreateController();
        controller.ModelState.AddModelError("error", "some error");

        // Act
        var result = await controller.Edit(_UserForSuccess);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("CreateEdit", viewResult.ViewName);
        Assert.Contains("some error", viewResult.ViewData[ViewProperties.Error]?.ToString());

        Assert.False(controller.Response.Headers.TryGetValue(PJax.PushUrl, out var _));

        var model = Assert.IsType<UserModel>(viewResult.ViewData.Model, exactMatch: false);
        Assert.Equal(_UserForSuccess.Id, model.Id);

        _MockUserService.Verify(x => x.SaveUserAsync(It.IsAny<UserModel>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Post_Edit_WithGenericServiceError_ReturnsCreateEditViewWithError() {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.Edit(_UserForFailure);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("CreateEdit", viewResult.ViewName);
        Assert.Contains(Core.ErrorGeneric, viewResult.ViewData[ViewProperties.Error]?.ToString());

        Assert.False(controller.Response.Headers.TryGetValue(PJax.PushUrl, out var _));

        var model = Assert.IsType<UserModel>(viewResult.ViewData.Model, exactMatch: false);
        Assert.Equal(_UserForFailure.Id, model.Id);

        _MockUserService.Verify(x => x.SaveUserAsync(It.IsAny<UserModel>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Post_Edit_WithConcurrencyServiceError_ReturnsCreateEditViewWithError() {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.Edit(_UserForConcurrencyError);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("CreateEdit", viewResult.ViewName);
        Assert.Contains(Core.ErrorConcurrency, viewResult.ViewData[ViewProperties.Error]?.ToString());

        Assert.False(controller.Response.Headers.TryGetValue(PJax.PushUrl, out var _));

        var model = Assert.IsType<UserModel>(viewResult.ViewData.Model, exactMatch: false);
        Assert.Equal(_UserForConcurrencyError.Id, model.Id);

        _MockUserService.Verify(x => x.SaveUserAsync(It.IsAny<UserModel>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Post_Edit_WithInvalidIdServiceError_ReturnsCreateEditViewWithError() {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.Edit(_UserForInvalidIdError);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("CreateEdit", viewResult.ViewName);
        Assert.Contains(Core.ErrorInvalidId, viewResult.ViewData[ViewProperties.Error]?.ToString());

        Assert.False(controller.Response.Headers.TryGetValue(PJax.PushUrl, out var _));

        var model = Assert.IsType<UserModel>(viewResult.ViewData.Model, exactMatch: false);
        Assert.Equal(_UserForInvalidIdError.Id, model.Id);

        _MockUserService.Verify(x => x.SaveUserAsync(It.IsAny<UserModel>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Post_Edit_WithDuplicateEmail_ReturnsCreateEditViewWithError() {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.Edit(_UserForDuplicateEmailError);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("CreateEdit", viewResult.ViewName);
        Assert.Contains(Users.ErrorDuplicateEmail, viewResult.ViewData[ViewProperties.Error]?.ToString());

        Assert.False(controller.Response.Headers.TryGetValue(PJax.PushUrl, out var _));

        var model = Assert.IsType<UserModel>(viewResult.ViewData.Model, exactMatch: false);
        Assert.Equal(_UserForDuplicateEmailError.Id, model.Id);

        _MockUserService.Verify(x => x.SaveUserAsync(It.IsAny<UserModel>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_Delete_WithValidId_ReturnsIndexViewWithMessage() {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.Delete(_UserForSuccess.Id);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", viewResult.ViewName);
        Assert.Equal(Users.SuccessDeletingUser, viewResult.ViewData[ViewProperties.Message]?.ToString());
        Assert.Null(viewResult.ViewData.Model);

        _MockUserService.Verify(x => x.DeleteUserAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_Delete_WithInvalidId_ReturnsIndexViewWithError() {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.Delete(_UserForFailure.Id);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", viewResult.ViewName);
        Assert.Equal(Users.ErrorDeletingUser, viewResult.ViewData[ViewProperties.Error]?.ToString());
        Assert.Null(viewResult.ViewData.Model);

        _MockUserService.Verify(x => x.DeleteUserAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
