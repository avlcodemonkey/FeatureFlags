using FeatureFlags.Constants;
using FeatureFlags.Controllers;
using FeatureFlags.Resources;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace FeatureFlags.Web.Tests.Controllers;

/// <summary>
/// Error controller doesn't have much to it yet.  But it probably will, so stub this out.
/// </summary>
public class ErrorControllerTests() {
    private readonly Mock<ILogger<ErrorController>> _MockLogger = new();

    private ErrorController CreateController(HttpContext? httpContext = null) {
        _MockLogger.Setup(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => true),
            It.IsAny<Exception>(), It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)));

        return new ErrorController(_MockLogger.Object) {
            ControllerContext = new ControllerContext {
                HttpContext = httpContext ?? new DefaultHttpContext()
            }
        };
    }

    [Fact]
    public void Get_Index_WithNoCode_ReturnsView() {
        // Arrange
        var controller = CreateController();

        // Act
        var result = controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Error", viewResult.ViewName);

        _MockLogger.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => true),
            It.IsAny<Exception>(), It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)), Times.Never
        );
    }

    [Fact]
    public void Get_Index_WithCode_LogsErrorAndReturnsView() {
        // Arrange
        var controller = CreateController();

        // Act
        var result = controller.Index("code");

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Error", viewResult.ViewName);

        _MockLogger.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => true),
            It.IsAny<Exception>(), It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)), Times.Once
        );
    }

    [Fact]
    public void Get_AccessDenied_ReturnsView() {
        // Arrange
        var controller = CreateController();

        // Act
        var result = controller.AccessDenied();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Error", viewResult.ViewName);
        Assert.Equal(Core.ErrorGeneric, viewResult.ViewData[ViewProperties.Message]?.ToString());
        Assert.Null(viewResult.ViewData.Model);
    }


    [Fact]
    public void Get_Index_AjaxRequest_ReturnsBadRequest() {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers.XRequestedWith = "XMLHttpRequest";
        var controller = CreateController(context);

        // Act
        var result = controller.Index();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        Assert.Equal(Core.ErrorGeneric, objectResult.Value);
    }

    [Fact]
    public void Get_AccessDenied_AjaxRequest_ReturnsForbidden() {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers.XRequestedWith = "XMLHttpRequest";
        var controller = CreateController(context);

        // Act
        var result = controller.AccessDenied();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, objectResult.StatusCode);
        Assert.Equal(Core.ErrorAccessDenied, objectResult.Value);
    }
}
