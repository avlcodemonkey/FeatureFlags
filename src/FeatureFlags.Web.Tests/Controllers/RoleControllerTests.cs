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

public class RoleControllerTests {
    private readonly Mock<IRoleService> _MockRoleService = new();
    private readonly Mock<IPermissionService> _MockPermissionService = new();
    private readonly Mock<IAssemblyService> _MockAssemblyService = new();
    private readonly Mock<ILogger<RoleController>> _MockLogger = new();
    private readonly Mock<IUrlHelper> _MockUrlHelper = new();

    private readonly string _Url = "/test";

    private readonly RoleModel _DefaultRole = new() { Id = -1, Name = "default role" };
    private readonly RoleModel _RoleForSuccess = new() { Id = -100, Name = "success role" };
    private readonly RoleModel _RoleForFailure = new() { Id = -101, Name = "failure role" };
    private readonly RoleModel _RoleForConcurrencyError = new() { Id = -102, Name = "concurrency role" };
    private readonly RoleModel _RoleForInvalidIdError = new() { Id = -103, Name = "invalid id role" };
    private readonly RoleModel _RoleForDuplicateName = new() { Id = -104, Name = "duplicate name role" };
    private readonly RoleModel _RoleForDuplicateDefault = new() { Id = -105, Name = "duplicate default role", IsDefault = true };

    private readonly CopyRoleModel _CopyRoleForSuccess = new() { Id = -100, Prompt = "success prompt" };
    private readonly CopyRoleModel _CopyRoleForFailure = new() { Id = -101, Prompt = "failure prompt" };
    private readonly CopyRoleModel _CopyRoleForDuplicateName = new() { Id = -102, Prompt = "default role" };

    public RoleControllerTests() {
        _MockRoleService.Setup(x => x.GetDefaultRoleAsync(It.IsAny<CancellationToken>())).ReturnsAsync(_DefaultRole);
        _MockRoleService.Setup(x => x.GetRoleByNameAsync(_RoleForDuplicateName.Name, It.IsAny<CancellationToken>())).ReturnsAsync(_RoleForDuplicateName);
        _MockRoleService.Setup(x => x.GetRoleByIdAsync(_RoleForSuccess.Id, It.IsAny<CancellationToken>())).ReturnsAsync(_RoleForSuccess);
        _MockRoleService.Setup(x => x.GetAllRolesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<RoleModel> { _RoleForSuccess, _RoleForFailure });

        _MockRoleService.Setup(x => x.SaveRoleAsync(_RoleForSuccess, It.IsAny<CancellationToken>())).ReturnsAsync((true, Roles.SuccessSavingRole));
        _MockRoleService.Setup(x => x.SaveRoleAsync(_RoleForFailure, It.IsAny<CancellationToken>())).ReturnsAsync((false, Core.ErrorGeneric));
        _MockRoleService.Setup(x => x.SaveRoleAsync(_RoleForConcurrencyError, It.IsAny<CancellationToken>())).ReturnsAsync((false, Core.ErrorConcurrency));
        _MockRoleService.Setup(x => x.SaveRoleAsync(_RoleForInvalidIdError, It.IsAny<CancellationToken>())).ReturnsAsync((false, Core.ErrorInvalidId));
        _MockRoleService.Setup(x => x.SaveRoleAsync(_RoleForDuplicateName, It.IsAny<CancellationToken>())).ReturnsAsync((false, Roles.ErrorDuplicateName));
        _MockRoleService.Setup(x => x.SaveRoleAsync(_RoleForDuplicateDefault, It.IsAny<CancellationToken>())).ReturnsAsync((false, Roles.ErrorDuplicateDefault));

        _MockRoleService.Setup(x => x.CopyRoleAsync(_CopyRoleForSuccess, It.IsAny<CancellationToken>())).ReturnsAsync((true, Roles.SuccessSavingRole));
        _MockRoleService.Setup(x => x.CopyRoleAsync(_CopyRoleForFailure, It.IsAny<CancellationToken>())).ReturnsAsync((false, Core.ErrorGeneric));
        _MockRoleService.Setup(x => x.CopyRoleAsync(_CopyRoleForDuplicateName, It.IsAny<CancellationToken>())).ReturnsAsync((false, Roles.ErrorDuplicateName));

        _MockRoleService.Setup(x => x.DeleteRoleAsync(_RoleForSuccess.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _MockRoleService.Setup(x => x.DeleteRoleAsync(_RoleForFailure.Id, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        _MockRoleService.Setup(x => x.AddPermissionsToDefaultRoleAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        _MockAssemblyService.Setup(x => x.GetActionList()).Returns([]);

        _MockPermissionService.Setup(x => x.GetAllPermissionsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PermissionModel>());
        _MockPermissionService.Setup(x => x.SavePermissionAsync(It.IsAny<PermissionModel>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _MockPermissionService.Setup(x => x.DeletePermissionAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        _MockUrlHelper.Setup(x => x.Action(It.IsAny<UrlActionContext>())).Returns(_Url);
    }

    private RoleController CreateController() => new(_MockRoleService.Object, _MockPermissionService.Object, _MockAssemblyService.Object, _MockLogger.Object) {
        ControllerContext = new ControllerContext {
            HttpContext = new DefaultHttpContext()
        },
        Url = _MockUrlHelper.Object
    };

    [Fact]
    public async Task Get_Index_ReturnsView_WithNoModel() {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", viewResult.ViewName);
        Assert.Null(viewResult.ViewData.Model);
        Assert.Null(viewResult.ViewData[ViewProperties.Error]);
    }

    [Fact]
    public async Task Get_Index_WithNoDefaultRole_ReturnsViewWithError() {
        // Arrange
        var mockRoleService = new Mock<IRoleService>();
        var controller = new RoleController(mockRoleService.Object, _MockPermissionService.Object, _MockAssemblyService.Object, _MockLogger.Object) {
            ControllerContext = new ControllerContext {
                HttpContext = new DefaultHttpContext()
            },
            Url = _MockUrlHelper.Object
        };

        // Act
        var result = await controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", viewResult.ViewName);
        Assert.Equal(Roles.ErrorNoDefaultRole, viewResult.ViewData[ViewProperties.Error]);
    }



    [Fact]
    public async Task Get_List_ReturnsData() {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.List();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<IEnumerable<RoleListResultModel>>(okResult.Value, exactMatch: false);
        Assert.Collection(returnValue,
            x => Assert.Equal(_RoleForSuccess.Id, x.Id),
            x => Assert.Equal(_RoleForFailure.Id, x.Id)
        );
        Assert.Collection(returnValue,
            x => Assert.Equal(_RoleForSuccess.Name, x.Name),
            x => Assert.Equal(_RoleForFailure.Name, x.Name)
        );
    }

    [Fact]
    public void Get_Create_ReturnsView_WithNewRoleModel() {
        // Arrange
        var controller = CreateController();

        // Act
        var result = controller.Create();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("CreateEdit", viewResult.ViewName);

        var model = Assert.IsType<RoleModel>(viewResult.ViewData.Model, exactMatch: false);
        Assert.Equal(0, model.Id);
    }

    [Fact]
    public async Task Post_Create_WithValidModel_ReturnsIndexView() {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.Create(_RoleForSuccess);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", viewResult.ViewName);
        Assert.Equal(Roles.SuccessSavingRole, viewResult.ViewData[ViewProperties.Message]?.ToString());

        Assert.True(controller.Response.Headers.TryGetValue(PJax.PushUrl, out var headerUrl));
        Assert.Equal(_Url, headerUrl.ToString());

        _MockRoleService.Verify(x => x.SaveRoleAsync(It.IsAny<RoleModel>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Post_Create_WithInvalidModel_ReturnsCreateEditViewWithError() {
        // Arrange
        var controller = CreateController();
        controller.ModelState.AddModelError("error", "some error");

        // Act
        var result = await controller.Create(_RoleForSuccess);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("CreateEdit", viewResult.ViewName);
        Assert.Contains("some error", viewResult.ViewData[ViewProperties.Error]?.ToString());

        Assert.False(controller.Response.Headers.TryGetValue(PJax.PushUrl, out var _));

        var model = Assert.IsType<RoleModel>(viewResult.ViewData.Model, exactMatch: false);
        Assert.Equal(_RoleForSuccess.Id, model.Id);

        _MockRoleService.Verify(x => x.SaveRoleAsync(It.IsAny<RoleModel>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Post_Create_WithDuplicateDefaultRole_ReturnsCreateEditViewWithError() {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.Create(_RoleForDuplicateDefault);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("CreateEdit", viewResult.ViewName);
        Assert.Contains(Roles.ErrorDuplicateDefault, viewResult.ViewData[ViewProperties.Error]?.ToString());

        Assert.False(controller.Response.Headers.TryGetValue(PJax.PushUrl, out var _));

        var model = Assert.IsType<RoleModel>(viewResult.ViewData.Model, exactMatch: false);
        Assert.Equal(_RoleForDuplicateDefault.Id, model.Id);

        _MockRoleService.Verify(x => x.SaveRoleAsync(It.IsAny<RoleModel>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Post_Create_WithDuplicateName_ReturnsCreateEditViewWithError() {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.Create(_RoleForDuplicateName);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("CreateEdit", viewResult.ViewName);
        Assert.Contains(Roles.ErrorDuplicateName, viewResult.ViewData[ViewProperties.Error]?.ToString());

        Assert.False(controller.Response.Headers.TryGetValue(PJax.PushUrl, out var _));

        var model = Assert.IsType<RoleModel>(viewResult.ViewData.Model, exactMatch: false);
        Assert.Equal(_RoleForDuplicateName.Id, model.Id);

        _MockRoleService.Verify(x => x.SaveRoleAsync(It.IsAny<RoleModel>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Post_Create_WithServiceError_ReturnsCreateEditViewWithError() {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.Create(_RoleForFailure);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("CreateEdit", viewResult.ViewName);
        Assert.Contains(Core.ErrorGeneric, viewResult.ViewData[ViewProperties.Error]?.ToString());

        Assert.False(controller.Response.Headers.TryGetValue(PJax.PushUrl, out var _));

        var model = Assert.IsType<RoleModel>(viewResult.ViewData.Model, exactMatch: false);
        Assert.Equal(_RoleForFailure.Id, model.Id);

        _MockRoleService.Verify(x => x.SaveRoleAsync(It.IsAny<RoleModel>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Get_Edit_WithValidId_ReturnsCreateEditViewWithModel() {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.Edit(_RoleForSuccess.Id);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("CreateEdit", viewResult.ViewName);

        var model = Assert.IsType<RoleModel>(viewResult.ViewData.Model, exactMatch: false);
        Assert.Equal(_RoleForSuccess.Id, model.Id);
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
        var result = await controller.Edit(_RoleForSuccess);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", viewResult.ViewName);
        Assert.Equal(Roles.SuccessSavingRole, viewResult.ViewData[ViewProperties.Message]?.ToString());
        Assert.Null(viewResult.ViewData.Model);

        Assert.True(controller.Response.Headers.TryGetValue(PJax.PushUrl, out var headerUrl));
        Assert.Equal(_Url, headerUrl.ToString());

        _MockRoleService.Verify(x => x.SaveRoleAsync(It.IsAny<RoleModel>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Post_Edit_WithInvalidModel_ReturnsCreateEditViewWithError() {
        // Arrange
        var controller = CreateController();
        controller.ModelState.AddModelError("error", "some error");

        // Act
        var result = await controller.Edit(_RoleForSuccess);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("CreateEdit", viewResult.ViewName);
        Assert.Contains("some error", viewResult.ViewData[ViewProperties.Error]?.ToString());

        Assert.False(controller.Response.Headers.TryGetValue(PJax.PushUrl, out var _));

        var model = Assert.IsType<RoleModel>(viewResult.ViewData.Model, exactMatch: false);
        Assert.Equal(_RoleForSuccess.Id, model.Id);

        _MockRoleService.Verify(x => x.SaveRoleAsync(It.IsAny<RoleModel>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Post_Edit_WithDuplicateDefaultRole_ReturnsCreateEditViewWithError() {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.Edit(_RoleForDuplicateDefault);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("CreateEdit", viewResult.ViewName);
        Assert.Contains(Roles.ErrorDuplicateDefault, viewResult.ViewData[ViewProperties.Error]?.ToString());

        Assert.False(controller.Response.Headers.TryGetValue(PJax.PushUrl, out var _));

        var model = Assert.IsType<RoleModel>(viewResult.ViewData.Model, exactMatch: false);
        Assert.Equal(_RoleForDuplicateDefault.Id, model.Id);

        _MockRoleService.Verify(x => x.SaveRoleAsync(It.IsAny<RoleModel>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Post_Edit_WithDuplicateName_ReturnsCreateEditViewWithError() {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.Edit(_RoleForDuplicateName);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("CreateEdit", viewResult.ViewName);
        Assert.Contains(Roles.ErrorDuplicateName, viewResult.ViewData[ViewProperties.Error]?.ToString());

        Assert.False(controller.Response.Headers.TryGetValue(PJax.PushUrl, out var _));

        var model = Assert.IsType<RoleModel>(viewResult.ViewData.Model, exactMatch: false);
        Assert.Equal(_RoleForDuplicateName.Id, model.Id);

        _MockRoleService.Verify(x => x.SaveRoleAsync(It.IsAny<RoleModel>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Post_Edit_WithGenericServiceError_ReturnsCreateEditViewWithError() {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.Edit(_RoleForFailure);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("CreateEdit", viewResult.ViewName);
        Assert.Contains(Core.ErrorGeneric, viewResult.ViewData[ViewProperties.Error]?.ToString());

        Assert.False(controller.Response.Headers.TryGetValue(PJax.PushUrl, out var _));

        var model = Assert.IsType<RoleModel>(viewResult.ViewData.Model, exactMatch: false);
        Assert.Equal(_RoleForFailure.Id, model.Id);

        _MockRoleService.Verify(x => x.SaveRoleAsync(It.IsAny<RoleModel>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Post_Edit_WithConcurrencyServiceError_ReturnsCreateEditViewWithError() {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.Edit(_RoleForConcurrencyError);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("CreateEdit", viewResult.ViewName);
        Assert.Contains(Core.ErrorConcurrency, viewResult.ViewData[ViewProperties.Error]?.ToString());

        Assert.False(controller.Response.Headers.TryGetValue(PJax.PushUrl, out var _));

        var model = Assert.IsType<RoleModel>(viewResult.ViewData.Model, exactMatch: false);
        Assert.Equal(_RoleForConcurrencyError.Id, model.Id);

        _MockRoleService.Verify(x => x.SaveRoleAsync(It.IsAny<RoleModel>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Post_Edit_WithInvalidIdServiceError_ReturnsCreateEditViewWithError() {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.Edit(_RoleForInvalidIdError);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("CreateEdit", viewResult.ViewName);
        Assert.Contains(Core.ErrorInvalidId, viewResult.ViewData[ViewProperties.Error]?.ToString());

        Assert.False(controller.Response.Headers.TryGetValue(PJax.PushUrl, out var _));

        var model = Assert.IsType<RoleModel>(viewResult.ViewData.Model, exactMatch: false);
        Assert.Equal(_RoleForInvalidIdError.Id, model.Id);

        _MockRoleService.Verify(x => x.SaveRoleAsync(It.IsAny<RoleModel>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Get_Copy_WithValidId_ReturnsCopyViewWithModel() {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.Copy(_RoleForSuccess.Id);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Copy", viewResult.ViewName);

        var model = Assert.IsType<CopyRoleModel>(viewResult.ViewData.Model, exactMatch: false);
        Assert.Equal(_RoleForSuccess.Id, model.Id);
    }

    [Fact]
    public async Task Get_Copy_WithInvalidId_ReturnsIndexViewWithError() {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.Copy(-200);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", viewResult.ViewName);
        Assert.Equal(Core.ErrorInvalidId, viewResult.ViewData[ViewProperties.Error]?.ToString());
        Assert.Null(viewResult.ViewData.Model);
    }

    [Fact]
    public async Task Post_Copy_WithValidModel_ReturnsIndexView() {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.Copy(_CopyRoleForSuccess);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", viewResult.ViewName);
        Assert.Equal(Roles.SuccessCopyingRole, viewResult.ViewData[ViewProperties.Message]?.ToString());

        Assert.True(controller.Response.Headers.TryGetValue(PJax.PushUrl, out var headerUrl));
        Assert.Equal(_Url, headerUrl.ToString());

        Assert.Null(viewResult.ViewData.Model);

        _MockRoleService.Verify(x => x.CopyRoleAsync(It.IsAny<CopyRoleModel>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Post_Copy_WithInvalidModel_ReturnsCopyViewWithError() {
        // Arrange
        var controller = CreateController();
        controller.ModelState.AddModelError("error", "some error");

        // Act
        var result = await controller.Copy(_CopyRoleForFailure);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Copy", viewResult.ViewName);
        Assert.Contains("some error", viewResult.ViewData[ViewProperties.Error]?.ToString());

        Assert.False(controller.Response.Headers.TryGetValue(PJax.PushUrl, out var _));

        _MockRoleService.Verify(x => x.GetRoleByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _MockRoleService.Verify(x => x.CopyRoleAsync(It.IsAny<CopyRoleModel>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Post_Copy_WithDuplicateName_ReturnsCopyViewWithError() {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.Copy(_CopyRoleForDuplicateName);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Copy", viewResult.ViewName);
        Assert.Contains(Roles.ErrorDuplicateName, viewResult.ViewData[ViewProperties.Error]?.ToString());

        Assert.False(controller.Response.Headers.TryGetValue(PJax.PushUrl, out var _));

        var model = Assert.IsType<CopyRoleModel>(viewResult.ViewData.Model, exactMatch: false);
        Assert.Equal(_CopyRoleForDuplicateName.Id, model.Id);

        _MockRoleService.Verify(x => x.CopyRoleAsync(It.IsAny<CopyRoleModel>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Post_Copy_WithServiceError_ReturnsCopyViewWithError() {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.Copy(_CopyRoleForFailure);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Copy", viewResult.ViewName);
        Assert.Contains(Core.ErrorGeneric, viewResult.ViewData[ViewProperties.Error]?.ToString());

        Assert.False(controller.Response.Headers.TryGetValue(PJax.PushUrl, out var _));

        _MockRoleService.Verify(x => x.CopyRoleAsync(It.IsAny<CopyRoleModel>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_Delete_WithValidId_ReturnsIndexViewWithMessage() {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.Delete(_RoleForSuccess.Id);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", viewResult.ViewName);
        Assert.Equal(Roles.SuccessDeletingRole, viewResult.ViewData[ViewProperties.Message]?.ToString());
        Assert.Null(viewResult.ViewData.Model);

        _MockRoleService.Verify(x => x.DeleteRoleAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_Delete_WithInvalidId_ReturnsIndexViewWithError() {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.Delete(_RoleForFailure.Id);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", viewResult.ViewName);
        Assert.Equal(Roles.ErrorDeletingRole, viewResult.ViewData[ViewProperties.Error]?.ToString());
        Assert.Null(viewResult.ViewData.Model);

        _MockRoleService.Verify(x => x.DeleteRoleAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Get_RefreshPermissions_ReturnsIndexViewWithMessage() {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.RefreshPermissions();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", viewResult.ViewName);
        Assert.Equal(Roles.SuccessRefreshingPermissions, viewResult.ViewData[ViewProperties.Message]?.ToString());
        Assert.Null(viewResult.ViewData.Model);
    }

    [Fact]
    public async Task Get_RefreshPermissions_WithServiceError_ReturnsIndexViewWithError() {
        // Arrange
        var controller = CreateController();
        _MockRoleService.Setup(x => x.AddPermissionsToDefaultRoleAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var result = await controller.RefreshPermissions();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", viewResult.ViewName);
        Assert.Equal(Roles.ErrorRefreshingPermissions, viewResult.ViewData[ViewProperties.Error]?.ToString());
        Assert.Null(viewResult.ViewData.Model);
    }
}
