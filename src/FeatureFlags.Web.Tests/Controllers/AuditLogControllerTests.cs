using FeatureFlags.Constants;
using FeatureFlags.Controllers;
using FeatureFlags.Models;
using FeatureFlags.Resources;
using FeatureFlags.Services;
using FeatureFlags.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace FeatureFlags.Web.Tests.Controllers;

public class AuditLogControllerTests() {
    private readonly Mock<IUserService> _MockUserService = new();
    private readonly Mock<IAuditLogService> _MockAuditLogService = new();
    private readonly Mock<ILogger<AuditLogController>> _MockLogger = new();

    private readonly AuditLogModel _AuditLogForSuccess = new() { Id = -100L, BatchId = Guid.NewGuid() };
    private readonly AuditLogModel _AuditLogForFailure = new() { Id = -101L, BatchId = Guid.NewGuid() };

    private AuditLogController CreateController() {
        _MockAuditLogService.Setup(x => x.GetLogByIdAsync(_AuditLogForSuccess.Id, CancellationToken.None)).ReturnsAsync(_AuditLogForSuccess);
        _MockAuditLogService.Setup(x => x.GetLogByIdAsync(_AuditLogForFailure.Id, CancellationToken.None)).ReturnsAsync(null as AuditLogModel);

        return new AuditLogController(_MockAuditLogService.Object, _MockUserService.Object, _MockLogger.Object);
    }

    [Fact]
    public void Get_Index_ReturnsView_WithSearchModel() {
        // Arrange
        var auditLogSearchModel = new AuditLogSearchModel { BatchId = Guid.NewGuid() };

        var controller = CreateController();

        // Act
        var result = controller.Index(auditLogSearchModel);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", viewResult.ViewName);

        var model = Assert.IsType<AuditLogSearchModel>(viewResult.ViewData.Model, exactMatch: false);
        Assert.Equal(auditLogSearchModel.BatchId, model.BatchId);
    }

    [Fact]
    public async Task Get_View_WithValidId_ReturnsView() {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.View(_AuditLogForSuccess.Id);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("View", viewResult.ViewName);

        var model = Assert.IsType<AuditLogModel>(viewResult.ViewData.Model, exactMatch: false);
        Assert.Equal(_AuditLogForSuccess.BatchId, model.BatchId);
    }

    [Fact]
    public async Task Get_View_WithInvalidId_ReturnsIndexWithError() {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.View(_AuditLogForFailure.Id);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", viewResult.ViewName);
        Assert.Equal(Core.ErrorInvalidId, viewResult.ViewData[ViewProperties.Error]);
        _ = Assert.IsType<AuditLogSearchModel>(viewResult.ViewData.Model, exactMatch: false);
    }

    [Fact]
    public async Task Get_UserList_WithNoQuery_ReturnsFullUserList() {
        // Arrange
        var user1 = new AutocompleteUserModel { Value = -1, Label = NameHelper.DisplayName("name", "email") };
        var user2 = new AutocompleteUserModel { Value = -2, Label = NameHelper.DisplayName("gib", "berish") };

        _MockUserService.Setup(x => x.FindAutocompleteUsersByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<AutocompleteUserModel> { user1, user2 });
        var controller = CreateController();

        // Act
        var result = await controller.UserList("");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<IEnumerable<AutocompleteUserModel>>(okResult.Value, exactMatch: false);
        Assert.Equal(2, returnValue.Count());
        Assert.Collection(returnValue,
            x => Assert.Equal(user1.Value, x.Value),
            x => Assert.Equal(user2.Value, x.Value)
        );
        Assert.Collection(returnValue,
            x => Assert.Equal(user1.Label, x.Label),
            x => Assert.Equal(user2.Label, x.Label)
        );
    }

    [Fact]
    public async Task Get_UserList_WithQuery_ReturnsMatchingUserList() {
        // Arrange
        var user = new AutocompleteUserModel { Value = -2, Label = NameHelper.DisplayName("gib", "berish") };

        _MockUserService.Setup(x => x.FindAutocompleteUsersByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<AutocompleteUserModel> { user });
        var controller = CreateController();

        // Act
        var result = await controller.UserList("gib");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<IEnumerable<AutocompleteUserModel>>(okResult.Value, exactMatch: false);
        var resultUser = Assert.Single(returnValue);
        Assert.Equal(user.Value, resultUser.Value);
        Assert.Equal(user.Label, resultUser.Label);
    }

    [Fact]
    public async Task Post_Search_WithMatches_ReturnsResultList() {
        // Arrange
        var auditLogSearchResult = new AuditLogSearchResultModel { Id = 1L, BatchId = Guid.NewGuid() };
        var auditLogSearchModel = new AuditLogSearchModel { BatchId = auditLogSearchResult.BatchId, StartDate = null, EndDate = null };

        _MockAuditLogService.Setup(x => x.SearchLogsAsync(It.IsAny<AuditLogSearchModel>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<AuditLogSearchResultModel> { auditLogSearchResult });
        var controller = CreateController();

        // Act
        var result = await controller.Search(auditLogSearchModel);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<IEnumerable<AuditLogSearchResultModel>>(okResult.Value, exactMatch: false);
        var resultLog = Assert.Single(returnValue);
        Assert.Equal(auditLogSearchResult.Id, resultLog.Id);
    }

    [Fact]
    public async Task Post_Search_WithNoMatches_ReturnsEmptyResultList() {
        // Arrange
        var auditLogSearchModel = new AuditLogSearchModel { BatchId = Guid.NewGuid(), StartDate = null, EndDate = null };

        _MockAuditLogService.Setup(x => x.SearchLogsAsync(It.IsAny<AuditLogSearchModel>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<AuditLogSearchResultModel>());
        var controller = CreateController();

        // Act
        var result = await controller.Search(auditLogSearchModel);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<IEnumerable<AuditLogSearchResultModel>>(okResult.Value, exactMatch: false);
        Assert.Empty(returnValue);
    }
}
