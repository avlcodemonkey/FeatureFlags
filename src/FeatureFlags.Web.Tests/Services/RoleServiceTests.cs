using FeatureFlags.Domain.Models;
using FeatureFlags.Models;
using FeatureFlags.Resources;
using FeatureFlags.Services;
using FeatureFlags.Web.Tests.Fixtures;

namespace FeatureFlags.Web.Tests.Services;

[Collection(nameof(DatabaseCollection))]
public class RoleServiceTests(DatabaseFixture fixture) {
    private readonly DatabaseFixture _Fixture = fixture;
    private readonly RoleService _RoleService = new(fixture.CreateContext());

    private RoleService GetNewRoleService() => new(_Fixture.CreateContext());

    /// <summary>
    /// Delete a role so it doesn't interfere with other tests.
    /// </summary>
    private async Task DeleteRoleIfExistsAsync(int roleId) {
        var role = (await GetNewRoleService().GetAllRolesAsync()).FirstOrDefault(x => x.Id == roleId);
        if (role != null) {
            // need a new context for this to avoid concurrency error
            await GetNewRoleService().DeleteRoleAsync(role.Id);
        }
    }

    [Fact]
    public async Task GetDefaultRoleAsync_ReturnsAdminRoleModel() {
        // arrange - no need to do anything since the model builder creates a default role

        // act
        var defaultRole = await _RoleService.GetDefaultRoleAsync();

        // assert
        Assert.NotNull(defaultRole);
        Assert.IsType<RoleModel>(defaultRole);
        Assert.Equal("Administrator", defaultRole.Name);
        Assert.True(defaultRole.IsDefault);
    }

    [Fact]
    public async Task GetRoleByIdAsync_ReturnsTestRoleModel() {
        // arrange
        var testRole = _Fixture.TestRole;

        // act
        var role = await _RoleService.GetRoleByIdAsync(testRole.Id);

        // assert
        Assert.NotNull(role);
        Assert.IsType<RoleModel>(role);
        Assert.Equal(testRole.Name, role.Name);
        Assert.False(role.IsDefault);
        Assert.NotNull(role.PermissionIds);
        Assert.Equal(testRole.RolePermissions.Count, role.PermissionIds.Count());
        Assert.Equal(testRole.RolePermissions[0].PermissionId, role.PermissionIds.First());
    }

    [Fact]
    public async Task GetRoleByNameAsync_WithTestRoleName_ReturnsTestRoleModel() {
        // arrange
        var testRole = _Fixture.TestRole;

        // act
        var role = await _RoleService.GetRoleByNameAsync(testRole.Name);

        // assert
        Assert.NotNull(role);
        Assert.IsType<RoleModel>(role);
        Assert.Equal(testRole.Id, role.Id);
        Assert.False(role.IsDefault);
        Assert.NotNull(role.PermissionIds);
        Assert.Equal(testRole.RolePermissions.Count, role.PermissionIds.Count());
        Assert.Equal(testRole.RolePermissions[0].PermissionId, role.PermissionIds.First());
    }

    [Fact]
    public async Task GetRoleByNameAsync_WithFakeName_ReturnsNull() {
        // act
        var role = await _RoleService.GetRoleByNameAsync("gibberish");

        // assert
        Assert.Null(role);
    }

    [Fact]
    public async Task GetRoleByIdAsync_WithInvalidRoleId_ReturnsNull() {
        // arrange
        var roleIdToGet = -200;

        // act
        var result = await _RoleService.GetRoleByIdAsync(roleIdToGet);

        // assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllRolesAsync_ReturnsListOfRoleModels() {
        // arrange
        var testRole = _Fixture.TestRole;

        // act
        var roles = await _RoleService.GetAllRolesAsync();

        // assert
        Assert.NotEmpty(roles);
        Assert.IsType<IEnumerable<RoleModel>>(roles, exactMatch: false);
        Assert.Collection(roles,
            x => Assert.Equal(testRole.Id, x.Id),
            x => Assert.Equal(1, x.Id)
        );
        Assert.Contains(roles, x => x.Id == testRole.Id);
    }

    [Fact]
    public async Task SaveRoleAsync_CreateRole_SavesNewRole() {
        // arrange
        var testPermission = _Fixture.TestPermission;
        var createRole = new RoleModel {
            Name = "Create test", IsDefault = false,
            PermissionIds = [testPermission.Id]
        };

        // act
        (var success, var message) = await _RoleService.SaveRoleAsync(createRole);
        var newRole = (await _RoleService.GetAllRolesAsync()).FirstOrDefault(x => x.Name == createRole.Name);
        if (newRole != null) {
            // delete the newly created role so it doesn't interfere with other tests
            await _RoleService.DeleteRoleAsync(newRole.Id);
        }

        // assert
        Assert.True(success);
        Assert.Equal(Roles.SuccessSavingRole, message);
        Assert.NotNull(newRole);
        Assert.NotNull(newRole.PermissionIds);
        Assert.Equal(createRole.PermissionIds.Count(), newRole.PermissionIds.Count());
    }

    [Fact]
    public async Task SaveRoleAsync_UpdateRole_SavesChanges() {
        // arrange
        var testPermission = _Fixture.TestPermission;
        var originalName = "original name";
        var createRole = new RoleModel {
            Name = originalName, IsDefault = false,
            PermissionIds = [testPermission.Id]
        };
        await _RoleService.SaveRoleAsync(createRole);

        var roleCopy = (await _RoleService.GetAllRolesAsync()).FirstOrDefault(x => x.Name == originalName);
        var newRoleName = "new name";
        var updateRole = roleCopy! with { Name = newRoleName, PermissionIds = [] };

        // act
        (var success, var message) = await _RoleService.SaveRoleAsync(updateRole);
        var updatedRole = (await _RoleService.GetAllRolesAsync()).FirstOrDefault(x => x.Id == roleCopy.Id);
        if (updatedRole != null) {
            // delete the newly created role so it doesn't interfere with other tests
            // need a new context for this to avoid concurrency error
            await GetNewRoleService().DeleteRoleAsync(updatedRole.Id);
        }

        // assert
        Assert.True(success);
        Assert.Equal(Roles.SuccessSavingRole, message);
        Assert.NotNull(updatedRole);
        Assert.Equal(newRoleName, updatedRole.Name);
        Assert.NotNull(updatedRole.PermissionIds);
        Assert.Empty(updatedRole.PermissionIds);
    }

    [Fact]
    public async Task SaveRoleAsync_UpdateInvalidRoleId_ReturnsInvalidIdError() {
        // arrange
        var updateRole = new RoleModel {
            Id = 999, Name = "Update role", IsDefault = false,
            PermissionIds = []
        };

        // act
        (var success, var message) = await _RoleService.SaveRoleAsync(updateRole);

        // assert
        Assert.False(success);
        Assert.Equal(Core.ErrorInvalidId, message);
    }

    [Fact]
    public async Task SaveRoleAsync_CreateWithDuplicateName_ReturnsDuplicateError() {
        // arrange
        var roleName = _Fixture.TestRole.Name;
        var duplicateRole = new RoleModel {
            Name = roleName, IsDefault = false,
            PermissionIds = []
        };

        // act
        (var success, var message) = await _RoleService.SaveRoleAsync(duplicateRole);

        // assert
        Assert.False(success);
        Assert.Equal(Roles.ErrorDuplicateName, message);
    }

    [Fact]
    public async Task SaveRoleAsync_UpdateWithDuplicateName_ReturnsDuplicateError() {
        // arrange
        var originalName = "duplicate original";
        var duplicateRole = new RoleModel {
            Name = originalName, IsDefault = false, PermissionIds = []
        };
        await _RoleService.SaveRoleAsync(duplicateRole);

        var roleCopy = await _RoleService.GetRoleByNameAsync(originalName);
        var updateRole = roleCopy! with { Name = _Fixture.TestRole.Name };

        // act
        (var success, var message) = await _RoleService.SaveRoleAsync(updateRole);

        // assert
        Assert.False(success);
        Assert.Equal(Roles.ErrorDuplicateName, message);
    }

    [Fact]
    public async Task SaveRoleAsync_WithConcurrentChanges_ReturnsConcurrencyError() {
        // arrange
        var testPermission = _Fixture.TestPermission;
        var originalName = "concurrency1 original name";
        var createRole = new RoleModel {
            Name = originalName, IsDefault = false,
            PermissionIds = [testPermission.Id]
        };
        await _RoleService.SaveRoleAsync(createRole);

        var roleCopy = (await _RoleService.GetAllRolesAsync()).First(x => x.Name == originalName);
        // decrement UpdatedDate to emulate an older row version
        var updateRole = roleCopy with { Name = "concurrency1 new name", UpdatedDate = roleCopy.UpdatedDate.AddSeconds(-10) };

        // act
        (var success, var message) = await GetNewRoleService().SaveRoleAsync(updateRole);

        await DeleteRoleIfExistsAsync(roleCopy.Id);

        // assert
        Assert.False(success);
        Assert.Equal(Core.ErrorConcurrency, message);
    }

    [Fact]
    public async Task SaveRoleAsync_WithSameUpdatedDateAndConcurrentChanges_ReturnsConcurrencyError() {
        // arrange
        var testPermission = _Fixture.TestPermission;
        var originalName = "concurrency2 original name";
        var createRole = new RoleModel {
            Name = originalName, IsDefault = false,
            PermissionIds = [testPermission.Id]
        };
        await _RoleService.SaveRoleAsync(createRole);

        var roleCopy = (await _RoleService.GetAllRolesAsync()).First(x => x.Name == originalName);
        var updateRole = roleCopy with { Name = "concurrency2 new name" };
        var finalRole = roleCopy with { Name = "concurrency2 final name" };
        await Task.Delay(1100);

        // act
        (var success, var message) = await GetNewRoleService().SaveRoleAsync(updateRole);
        (var success2, var message2) = await GetNewRoleService().SaveRoleAsync(finalRole);

        await DeleteRoleIfExistsAsync(roleCopy.Id);

        // assert
        Assert.True(success);
        Assert.False(success2);
        Assert.Equal(Roles.SuccessSavingRole, message);
        Assert.Equal(Core.ErrorConcurrency, message2);
    }

    [Fact]
    public async Task SaveRoleAsync_WithIncrementedUpdatedDateAndConcurrentChanges_ReturnsSuccess() {
        // arrange
        var testPermission = _Fixture.TestPermission;
        var originalName = "concurrency3 original name";
        var createRole = new RoleModel {
            Name = originalName, IsDefault = false,
            PermissionIds = [testPermission.Id]
        };
        await _RoleService.SaveRoleAsync(createRole);

        var roleCopy = (await _RoleService.GetAllRolesAsync()).FirstOrDefault(x => x.Name == originalName);
        var updateRole = roleCopy! with { Name = "concurrency3 new name" };
        var finalRole = roleCopy with { Name = "concurrency3 final name", UpdatedDate = roleCopy.UpdatedDate.AddSeconds(10) };

        // act
        (var success, var message) = await _RoleService.SaveRoleAsync(updateRole);
        (var success2, var message2) = await _RoleService.SaveRoleAsync(finalRole);

        await DeleteRoleIfExistsAsync(roleCopy.Id);

        // assert
        Assert.True(success);
        Assert.True(success2);
        Assert.Equal(Roles.SuccessSavingRole, message);
        Assert.Equal(Roles.SuccessSavingRole, message2);
    }

    [Fact]
    public async Task CopyRoleAsync_WithValidRole_SavesNewRole() {
        // arrange
        var testRole = _Fixture.TestRole;
        var copyRole = new CopyRoleModel { Id = testRole.Id, Prompt = "Copy of test role" };

        // act
        (var success, var message) = await _RoleService.CopyRoleAsync(copyRole);
        var newRole = (await _RoleService.GetAllRolesAsync()).FirstOrDefault(x => x.Name == copyRole.Prompt);
        if (newRole != null) {
            // delete the copy so it doesn't interfere with other tests
            await _RoleService.DeleteRoleAsync(newRole.Id);
        }

        // assert
        Assert.True(success);
        Assert.Equal(Roles.SuccessCopyingRole, message);
        Assert.NotNull(newRole);
        Assert.Equal(copyRole.Prompt, newRole.Name);
        Assert.NotNull(newRole.PermissionIds);
        Assert.Equal(testRole.RolePermissions.Count, newRole.PermissionIds.Count());
    }

    [Fact]
    public async Task CopyRoleAsync_WithInvalidRoleId_ReturnsFalseWithError() {
        // arrange
        var copyRole = new CopyRoleModel { Id = -300, Prompt = "Copy" };

        // act
        (var success, var message) = await _RoleService.CopyRoleAsync(copyRole);

        // assert
        Assert.False(success);
        Assert.Equal(Core.ErrorInvalidId, message);
    }

    [Fact]
    public async Task CopyRoleAsync_WithDuplicateName_ReturnsFalseWithError() {
        // arrange
        var copyRole = new CopyRoleModel { Id = _Fixture.TestRole.Id, Prompt = _Fixture.TestRole.Name };

        // act
        (var success, var message) = await _RoleService.CopyRoleAsync(copyRole);

        // assert
        Assert.False(success);
        Assert.Equal(Roles.ErrorDuplicateName, message);
    }

    [Fact]
    public async Task DeleteRoleAsync_WithValidRole_DeletesRole() {
        // arrange
        var roleToDelete = new Role { Id = -100, Name = "Delete Test", IsDefault = false };
        int originalCount;
        using (var dbContext = _Fixture.CreateContext()) {
            dbContext.Roles.Add(roleToDelete);
            dbContext.SaveChanges();
            originalCount = dbContext.Roles.Count();
        }

        // act
        var result = await _RoleService.DeleteRoleAsync(roleToDelete.Id);
        var newCount = (await _RoleService.GetAllRolesAsync()).Count();

        // assert
        Assert.True(result);
        Assert.Equal(originalCount - 1, newCount);
    }

    [Fact]
    public async Task DeleteRoleAsync_WithInvalidRoleId_ReturnsFalse() {
        // arrange
        var roleIdToDelete = -200;

        // act
        var result = await _RoleService.DeleteRoleAsync(roleIdToDelete);

        // assert
        Assert.False(result);
    }
}
