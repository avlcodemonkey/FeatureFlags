using FeatureFlags.Client;
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

public class FeatureFlagControllerTests {
    private readonly Mock<IFeatureFlagService> _MockFeatureFlagService = new();
    private readonly Mock<ILogger<FeatureFlagController>> _MockLogger = new();
    private readonly Mock<IFeatureFlagClient> _MockFeatureFlagClient = new();
    private readonly Mock<IUrlHelper> _MockUrlHelper = new();

    private readonly string _Url = "/test";
    private readonly FeatureFlagModel _FlagForFailure = new() { Id = -101, Name = "failure", IsEnabled = true };
    private readonly FeatureFlagModel _FlagForFailureDisabled = new() { Id = -101, Name = "failure", IsEnabled = false };

    public FeatureFlagControllerTests() {
        _MockUrlHelper.Setup(x => x.Action(It.IsAny<UrlActionContext>())).Returns(_Url);
        _MockFeatureFlagService.Setup(x => x.GetFeatureFlagByIdAsync(_FlagForFailure.Id, It.IsAny<CancellationToken>())).ReturnsAsync(_FlagForFailure);
        _MockFeatureFlagService.Setup(x => x.SaveFeatureFlagAsync(_FlagForFailure, It.IsAny<CancellationToken>())).ReturnsAsync((false, "message"));
        _MockFeatureFlagService.Setup(x => x.SaveFeatureFlagAsync(_FlagForFailureDisabled, It.IsAny<CancellationToken>())).ReturnsAsync((false, "message"));
    }

    private FeatureFlagController CreateController() => new(_MockFeatureFlagService.Object, _MockFeatureFlagClient.Object, _MockLogger.Object) {
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
            new() { Id = 1, Name = "Flag1", IsEnabled = true },
            new() { Id = 2, Name = "Flag2", IsEnabled = false }
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
    public async Task Patch_Enable_ValidId_ReturnsIndexWithSuccessMessage() {
        // Arrange
        var controller = CreateController();
        var featureFlag = new FeatureFlagModel { Id = 1, Name = "Flag1", IsEnabled = false };
        _MockFeatureFlagService.Setup(service => service.GetFeatureFlagByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(featureFlag);
        _MockFeatureFlagService.Setup(service => service.SaveFeatureFlagAsync(It.IsAny<FeatureFlagModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, "Success"));

        // Act
        var result = await controller.Enable(1);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", viewResult.ViewName);
        Assert.NotNull(controller.ViewData[ViewProperties.Message]);
        Assert.Equal(Flags.SuccessSavingFlag, controller.ViewData[ViewProperties.Message]!.ToString());
    }

    [Fact]
    public async Task Patch_Enable_InvalidId_ReturnsIndexWithErrorMessage() {
        // Arrange
        var controller = CreateController();
        _MockFeatureFlagService.Setup(service => service.GetFeatureFlagByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(null as FeatureFlagModel);

        // Act
        var result = await controller.Enable(1);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", viewResult.ViewName);
        Assert.NotNull(controller.ViewData[ViewProperties.Error]);
        Assert.Equal(Core.ErrorInvalidId, controller.ViewData[ViewProperties.Error]!.ToString());
    }

    [Fact]
    public async Task Patch_Enable_WithServiceError_ReturnsIndexViewWithError() {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.Enable(_FlagForFailure.Id);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", viewResult.ViewName);
        Assert.Equal("message", viewResult.ViewData[ViewProperties.Error]?.ToString());
        Assert.True(controller.Response.Headers.TryGetValue(PJax.PushUrl, out var _));
        _MockFeatureFlagService.Verify(x => x.SaveFeatureFlagAsync(It.IsAny<FeatureFlagModel>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Patch_Disable_ValidId_ReturnsIndexWithSuccessMessage() {
        // Arrange
        var controller = CreateController();
        var featureFlag = new FeatureFlagModel { Id = 1, Name = "Flag1", IsEnabled = true };
        _MockFeatureFlagService.Setup(service => service.GetFeatureFlagByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(featureFlag);
        _MockFeatureFlagService.Setup(service => service.SaveFeatureFlagAsync(It.IsAny<FeatureFlagModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, "Success"));

        // Act
        var result = await controller.Disable(1);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", viewResult.ViewName);
        Assert.NotNull(controller.ViewData[ViewProperties.Message]);
        Assert.Equal(Flags.SuccessSavingFlag, controller.ViewData[ViewProperties.Message]!.ToString());
    }


    [Fact]
    public async Task Patch_Disable_InvalidId_ReturnsIndexWithErrorMessage() {
        // Arrange
        var controller = CreateController();
        _MockFeatureFlagService.Setup(service => service.GetFeatureFlagByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(null as FeatureFlagModel);

        // Act
        var result = await controller.Disable(1);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", viewResult.ViewName);
        Assert.NotNull(controller.ViewData[ViewProperties.Error]);
        Assert.Equal(Core.ErrorInvalidId, controller.ViewData[ViewProperties.Error]!.ToString());
    }

    [Fact]
    public async Task Patch_Disable_WithServiceError_ReturnsIndexViewWithError() {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.Disable(_FlagForFailure.Id);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", viewResult.ViewName);
        Assert.Equal("message", viewResult.ViewData[ViewProperties.Error]?.ToString());
        Assert.True(controller.Response.Headers.TryGetValue(PJax.PushUrl, out var _));
        _MockFeatureFlagService.Verify(x => x.SaveFeatureFlagAsync(It.IsAny<FeatureFlagModel>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Get_RefreshFlags_ReturnsIndexWithSuccessMessage() {
        // Arrange
        var controller = CreateController();
        var featureFlags = new List<FeatureFlagModel> {
            new() { Id = 1, Name = "Flag1", IsEnabled = true },
            new() { Id = 2, Name = "Flag2", IsEnabled = false }
        };
        _MockFeatureFlagService.Setup(service => service.GetAllFeatureFlagsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(featureFlags);
        _MockFeatureFlagService.Setup(service => service.SaveFeatureFlagAsync(It.IsAny<FeatureFlagModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, "Success"));
        _MockFeatureFlagService.Setup(service => service.DeleteFeatureFlagAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await controller.RefreshFlags();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", viewResult.ViewName);
        Assert.NotNull(controller.ViewData[ViewProperties.Message]);
        Assert.Equal(Flags.SuccessRefreshingFlags, controller.ViewData[ViewProperties.Message]!.ToString());
    }

    [Fact]
    public async Task Get_RefreshFlags_WithServiceError_ReturnsIndexWithErrorMessage() {
        // Arrange
        var controller = CreateController();
        var featureFlags = new List<FeatureFlagModel> {
            new() { Id = 1, Name = "Flag1", IsEnabled = true },
            new() { Id = 2, Name = "Flag2", IsEnabled = false }
        };
        _MockFeatureFlagService.Setup(service => service.GetAllFeatureFlagsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(featureFlags);
        _MockFeatureFlagService.Setup(service => service.SaveFeatureFlagAsync(It.IsAny<FeatureFlagModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, "error"));
        _MockFeatureFlagService.Setup(service => service.DeleteFeatureFlagAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await controller.RefreshFlags();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", viewResult.ViewName);
        Assert.NotNull(controller.ViewData[ViewProperties.Error]);
        Assert.Equal(Flags.ErrorRefreshingFlags, controller.ViewData[ViewProperties.Error]!.ToString());
    }

    [Fact]
    public void Get_ClearCache_ReturnsIndexWithSuccessMessage() {
        // Arrange
        var controller = CreateController();

        // Act
        var result = controller.ClearCache();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", viewResult.ViewName);
        Assert.NotNull(controller.ViewData[ViewProperties.Message]);
        Assert.Equal(Flags.SuccessClearingCache, controller.ViewData[ViewProperties.Message]!.ToString());
    }
}
