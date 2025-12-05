using FeatureFlags.Controllers;
using FeatureFlags.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;
using Moq;

namespace FeatureFlags.Web.Tests.Controllers;

/// <summary>
/// Dashboard controller doesn't have much to it yet.  But it will, so stub this out.
/// </summary>
public class DashboardControllerTests() {
    private readonly Mock<ILogger<DashboardController>> _MockLogger = new();
    private readonly Mock<IApiRequestService> _MockApiRequestService = new();
    private readonly Mock<IUserService> _MockUserService = new();
    private readonly Mock<IUrlHelper> _MockUrlHelper = new();

    [Fact]
    public void Get_Index_ReturnsView() {
        // Arrange
        _MockUrlHelper.Setup(x => x.Action(It.IsAny<UrlActionContext>())).Returns("/test-url");

        var controller = new DashboardController(_MockApiRequestService.Object, _MockUserService.Object, _MockLogger.Object) {
            Url = _MockUrlHelper.Object
        };

        // Act
        var result = controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", viewResult.ViewName);
    }
}
