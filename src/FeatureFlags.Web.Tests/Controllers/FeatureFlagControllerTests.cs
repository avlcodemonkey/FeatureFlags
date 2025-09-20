using FeatureFlags.Constants;
using FeatureFlags.Controllers;
using FeatureFlags.Models;
using FeatureFlags.Resources;
using FeatureFlags.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Moq;

namespace FeatureFlags.Web.Tests.Controllers;

public class FeatureFlagControllerTests {
    private readonly Mock<IFeatureFlagService> _MockFeatureFlagService = new();
    private readonly Mock<ILogger<FeatureFlagController>> _MockLogger = new();
    private readonly FeatureManager _MockFeatureManager;
    private readonly Mock<IUrlHelper> _MockUrlHelper = new();

    private readonly string _Url = "/test";
    private readonly FeatureFlagModel _FlagForFailure = new() { Id = -101, Name = "failure", Status = true };
    private readonly FeatureFlagModel _FlagForFailureDisabled = new() { Id = -101, Name = "failure", Status = false };

    public FeatureFlagControllerTests() {
        _MockUrlHelper.Setup(x => x.Action(It.IsAny<UrlActionContext>())).Returns(_Url);
        _MockFeatureFlagService.Setup(x => x.GetFeatureFlagByIdAsync(_FlagForFailure.Id, It.IsAny<CancellationToken>())).ReturnsAsync(_FlagForFailure);
        _MockFeatureFlagService.Setup(x => x.SaveFeatureFlagAsync(_FlagForFailure, It.IsAny<CancellationToken>())).ReturnsAsync((false, "message"));
        _MockFeatureFlagService.Setup(x => x.SaveFeatureFlagAsync(_FlagForFailureDisabled, It.IsAny<CancellationToken>())).ReturnsAsync((false, "message"));
        _MockFeatureManager = new FeatureManager(new Mock<IFeatureDefinitionProvider>().Object);
    }

    private FeatureFlagController CreateController() => new(_MockFeatureFlagService.Object, _MockFeatureManager, _MockLogger.Object) {
        ControllerContext = new ControllerContext {
            HttpContext = new DefaultHttpContext()
        },
        Url = _MockUrlHelper.Object
    };

    [Fact]
    public void Get_Index_ReturnsViewResult() {
        // Arrange
        var controller = CreateController();

        // Act
        var result = controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", viewResult.ViewName);
    }

    [Fact]
    public async Task Get_List_ReturnsOkResultWithFeatureFlags() {
        // Arrange
        var controller = CreateController();
        var featureFlags = new List<FeatureFlagModel> {
            new() { Id = 1, Name = "Flag1", Status = true },
            new() { Id = 2, Name = "Flag2", Status = false }
        };
        _MockFeatureFlagService.Setup(service => service.GetAllFeatureFlagsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(featureFlags);

        // Act
        var result = await controller.List();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<IEnumerable<FeatureFlagListResultModel>>(okResult.Value, exactMatch: false);
        Assert.Equal(2, returnValue.Count());
    }

    [Fact]
    public void Get_Create_ReturnsViewResult() {
        // Arrange
        var controller = CreateController();

        // Act
        var result = controller.Create();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("CreateEdit", viewResult.ViewName);
    }

    [Fact]
    public async Task Post_Create_WithValidModel_Success_RedirectsToIndex() {
        // Arrange
        var controller = CreateController();
        var model = new FeatureFlagModel {
            Name = "NewFlag",
            Status = true,
            RequirementType = Constants.RequirementType.All
        };
        _MockFeatureFlagService.Setup(s => s.SaveFeatureFlagAsync(model, It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, Flags.SuccessSavingFlag));

        // Act
        var result = await controller.Create(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", viewResult.ViewName);
        Assert.Contains(Flags.SuccessSavingFlag, viewResult.ViewData[ViewProperties.Message]?.ToString());
    }

    [Fact]
    public async Task Post_Create_WithInvalidModel_ReturnsViewWithError() {
        // Arrange
        var controller = CreateController();
        controller.ModelState.AddModelError("Name", "Required");
        var model = new FeatureFlagModel {
            Name = "",
            Status = true,
            RequirementType = Constants.RequirementType.All
        };

        // Act
        var result = await controller.Create(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("CreateEdit", viewResult.ViewName);
        Assert.False(controller.ModelState.IsValid);
    }

    [Fact]
    public async Task Post_Create_ServiceFailure_ReturnsViewWithError() {
        // Arrange
        var controller = CreateController();
        var model = _FlagForFailure;

        // Act
        var result = await controller.Create(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("CreateEdit", viewResult.ViewName);
        Assert.Equal("message", controller.ViewData[ViewProperties.Error]);
    }

    [Fact]
    public async Task Get_Edit_WithValidId_ReturnsViewResult() {
        // Arrange
        var controller = CreateController();
        var model = new FeatureFlagModel {
            Id = 1,
            Name = "EditFlag",
            Status = true,
            RequirementType = Constants.RequirementType.Any
        };
        _MockFeatureFlagService.Setup(s => s.GetFeatureFlagByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(model);

        // Act
        var result = await controller.Edit(1);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("CreateEdit", viewResult.ViewName);
        Assert.Equal(model, viewResult.Model);
    }

    [Fact]
    public async Task Get_Edit_WithInvalidId_RedirectsToIndex() {
        // Arrange
        var controller = CreateController();
        _MockFeatureFlagService.Setup(s => s.GetFeatureFlagByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FeatureFlagModel?)null);

        // Act
        var result = await controller.Edit(999);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", viewResult.ViewName);
        Assert.Equal(Core.ErrorInvalidId, controller.ViewData[ViewProperties.Error]);
    }

    [Fact]
    public async Task Put_Edit_WithValidModel_Success_RedirectsToIndex() {
        // Arrange
        var controller = CreateController();
        var model = new FeatureFlagModel {
            Id = 1,
            Name = "EditFlag",
            Status = false,
            RequirementType = Constants.RequirementType.Any
        };
        _MockFeatureFlagService.Setup(s => s.SaveFeatureFlagAsync(model, It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, Flags.SuccessSavingFlag));

        // Act
        var result = await controller.Edit(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", viewResult.ViewName);
        Assert.Contains(Flags.SuccessSavingFlag, viewResult.ViewData[ViewProperties.Message]?.ToString());
    }

    [Fact]
    public async Task Put_Edit_WithInvalidModel_ReturnsViewWithError() {
        // Arrange
        var controller = CreateController();
        controller.ModelState.AddModelError("Name", "Required");
        var model = new FeatureFlagModel {
            Id = 1,
            Name = "",
            Status = false,
            RequirementType = Constants.RequirementType.Any
        };

        // Act
        var result = await controller.Edit(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("CreateEdit", viewResult.ViewName);
        Assert.False(controller.ModelState.IsValid);
    }

    [Fact]
    public async Task Put_Edit_ServiceFailure_ReturnsViewWithError() {
        // Arrange
        var controller = CreateController();
        var model = _FlagForFailureDisabled;

        // Act
        var result = await controller.Edit(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("CreateEdit", viewResult.ViewName);
        Assert.Equal("message", controller.ViewData[ViewProperties.Error]);
    }

    [Fact]
    public async Task Delete_WithValidId_Success_RedirectsToIndex() {
        // Arrange
        var controller = CreateController();
        _MockFeatureFlagService.Setup(s => s.DeleteFeatureFlagAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await controller.Delete(1);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", viewResult.ViewName);
        Assert.Equal(Flags.SuccessDeletingFlag, controller.ViewData[ViewProperties.Message]);
    }

    [Fact]
    public async Task Delete_WithInvalidId_ReturnsViewWithError() {
        // Arrange
        var controller = CreateController();
        _MockFeatureFlagService.Setup(s => s.DeleteFeatureFlagAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await controller.Delete(999);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", viewResult.ViewName);
        Assert.Equal(Flags.ErrorDeletingFlag, controller.ViewData[ViewProperties.Error]);
    }

    [Fact]
    public async Task List_ReturnsEmptyList_WhenNoFlags() {
        var controller = CreateController();
        _MockFeatureFlagService.Setup(s => s.GetAllFeatureFlagsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FeatureFlagModel>());

        var result = await controller.List();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<List<FeatureFlagListResultModel>>(okResult.Value);
        Assert.Empty(returnValue);
    }

    [Fact]
    public async Task Save_ReturnsFilterErrorMessage_WhenFilterValidationFails() {
        var controller = CreateController();
        controller.ModelState.AddModelError("Filters[0].SomeProperty", "Filter error");
        var model = new FeatureFlagModel {
            Name = "FlagWithFilterError",
            Status = true,
            RequirementType = Constants.RequirementType.All
        };

        var result = await controller.Create(model);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("CreateEdit", viewResult.ViewName);
        Assert.Contains(Flags.ErrorCheckFilters, controller.ViewData[ViewProperties.Error]?.ToString());
    }

    // @TODO: Add tests for dynamic feature manager evaluation logic
}
