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
        _MockFeatureFlagClient.Verify(x => x.ClearCache(), Times.Once);
    }
}
