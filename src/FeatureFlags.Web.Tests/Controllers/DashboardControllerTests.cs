using FeatureFlags.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace FeatureFlags.Web.Tests.Controllers;

/// <summary>
/// Dashboard controller doesn't have much to it yet.  But it will, so stub this out.
/// </summary>
public class DashboardControllerTests() {
    private readonly Mock<ILogger<DashboardController>> _MockLogger = new();

    [Fact]
    public void Get_Index_ReturnsView() {
        // Arrange
        var controller = new DashboardController(_MockLogger.Object);

        // Act
        var result = controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", viewResult.ViewName);
    }
}
