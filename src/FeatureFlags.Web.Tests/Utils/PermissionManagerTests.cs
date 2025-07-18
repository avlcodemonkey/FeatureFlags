using FeatureFlags.Models;
using FeatureFlags.Services;
using FeatureFlags.Utils;
using Moq;

namespace FeatureFlags.Web.Tests.Utils;

public class PermissionManagerTests {
    private readonly Dictionary<string, string> _ActionList = new() {
        { "controller1.action1", "Controller1.Action1" },
        { "controller1.action2", "Controller1.Action2" },
        { "controller2.action1", "Controller2.Action1" },
    };

    private readonly List<PermissionModel> _PermissionList = [
        new PermissionModel { ControllerName = "controller1", ActionName = "action1" },
        new PermissionModel { ControllerName = "controller1", ActionName = "action2" },
        new PermissionModel { ControllerName = "controller2", ActionName = "action1" },
    ];

    [Fact]
    public async Task Register_WithNoChanges_DoesNothing() {
        // arrange
        var mockAssemblyService = new Mock<IAssemblyService>();
        mockAssemblyService.Setup(x => x.GetActionList()).Returns(_ActionList);

        var mockPermissionService = new Mock<IPermissionService>();
        mockPermissionService.Setup(x => x.GetAllPermissionsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(_PermissionList);
        mockPermissionService.Setup(x => x.SavePermissionAsync(It.IsAny<PermissionModel>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        mockPermissionService.Setup(x => x.DeletePermissionAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var mockRoleService = new Mock<IRoleService>();
        mockRoleService.Setup(x => x.AddPermissionsToDefaultRoleAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var permissionManager = new PermissionManager(mockAssemblyService.Object, mockPermissionService.Object, mockRoleService.Object);

        // act
        var result = await permissionManager.RegisterAsync();

        // assert
        Assert.True(result);
        mockPermissionService.Verify(x => x.GetAllPermissionsAsync(It.IsAny<CancellationToken>()), Times.Once);
        mockPermissionService.Verify(x => x.SavePermissionAsync(It.IsAny<PermissionModel>(), It.IsAny<CancellationToken>()), Times.Never);
        mockPermissionService.Verify(x => x.DeletePermissionAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        mockRoleService.Verify(x => x.AddPermissionsToDefaultRoleAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Register_WithNewActions_AddsPermissions() {
        // arrange
        var mockAssemblyService = new Mock<IAssemblyService>();
        mockAssemblyService.Setup(x => x.GetActionList()).Returns(_ActionList);

        var mockPermissionService = new Mock<IPermissionService>();
        mockPermissionService.Setup(x => x.GetAllPermissionsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PermissionModel>());
        mockPermissionService.Setup(x => x.SavePermissionAsync(It.IsAny<PermissionModel>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        mockPermissionService.Setup(x => x.DeletePermissionAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var mockRoleService = new Mock<IRoleService>();
        mockRoleService.Setup(x => x.AddPermissionsToDefaultRoleAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var permissionManager = new PermissionManager(mockAssemblyService.Object, mockPermissionService.Object, mockRoleService.Object);

        // act
        var result = await permissionManager.RegisterAsync();

        // assert
        Assert.True(result);
        mockPermissionService.Verify(x => x.GetAllPermissionsAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
        mockPermissionService.Verify(x => x.SavePermissionAsync(It.IsAny<PermissionModel>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
        mockPermissionService.Verify(x => x.DeletePermissionAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        mockRoleService.Verify(x => x.AddPermissionsToDefaultRoleAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Register_WithDeletedActions_DeletesPermissions() {
        // arrange
        var mockAssemblyService = new Mock<IAssemblyService>();
        mockAssemblyService.Setup(x => x.GetActionList()).Returns([]);

        var mockPermissionService = new Mock<IPermissionService>();
        mockPermissionService.Setup(x => x.GetAllPermissionsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(_PermissionList);
        mockPermissionService.Setup(x => x.SavePermissionAsync(It.IsAny<PermissionModel>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        mockPermissionService.Setup(x => x.DeletePermissionAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var mockRoleService = new Mock<IRoleService>();
        mockRoleService.Setup(x => x.AddPermissionsToDefaultRoleAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var permissionManager = new PermissionManager(mockAssemblyService.Object, mockPermissionService.Object, mockRoleService.Object);

        // act
        var result = await permissionManager.RegisterAsync();

        // assert
        Assert.True(result);
        mockPermissionService.Verify(x => x.GetAllPermissionsAsync(It.IsAny<CancellationToken>()), Times.Once);
        mockPermissionService.Verify(x => x.SavePermissionAsync(It.IsAny<PermissionModel>(), It.IsAny<CancellationToken>()), Times.Never);
        mockPermissionService.Verify(x => x.DeletePermissionAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
        mockRoleService.Verify(x => x.AddPermissionsToDefaultRoleAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Register_WithServiceError_ReturnsFalse() {
        // arrange
        var mockAssemblyService = new Mock<IAssemblyService>();
        mockAssemblyService.Setup(x => x.GetActionList()).Returns([]);

        var mockPermissionService = new Mock<IPermissionService>();
        mockPermissionService.Setup(x => x.GetAllPermissionsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(_PermissionList);
        mockPermissionService.Setup(x => x.SavePermissionAsync(It.IsAny<PermissionModel>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        // returning false here will trigger the manager to return false
        mockPermissionService.Setup(x => x.DeletePermissionAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var mockRoleService = new Mock<IRoleService>();
        mockRoleService.Setup(x => x.AddPermissionsToDefaultRoleAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var permissionManager = new PermissionManager(mockAssemblyService.Object, mockPermissionService.Object, mockRoleService.Object);

        // act
        var result = await permissionManager.RegisterAsync();

        // assert
        Assert.False(result);
        mockPermissionService.Verify(x => x.GetAllPermissionsAsync(It.IsAny<CancellationToken>()), Times.Once);
        mockPermissionService.Verify(x => x.SavePermissionAsync(It.IsAny<PermissionModel>(), It.IsAny<CancellationToken>()), Times.Never);
        mockPermissionService.Verify(x => x.DeletePermissionAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
        mockRoleService.Verify(x => x.AddPermissionsToDefaultRoleAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
