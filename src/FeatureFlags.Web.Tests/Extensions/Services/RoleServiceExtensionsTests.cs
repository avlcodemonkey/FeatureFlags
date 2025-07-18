using FeatureFlags.Domain.Models;
using FeatureFlags.Extensions.Services;

namespace FeatureFlags.Web.Tests.Extensions.Services;

public class RoleServiceExtensionsTests {
    [Fact]
    public void SelectSingleAsModel_ReturnsProjectedModel() {
        // arrange
        var rolePermission = new RolePermission { Id = 1, RoleId = 2, PermissionId = 3 };
        var role = new Role { Id = 4, Name = "test role", IsDefault = true, RolePermissions = [rolePermission], UpdatedDate = DateTime.MaxValue };
        var roles = new List<Role> { role }.AsQueryable();

        // act
        var models = roles.SelectAsModel();

        // assert
        Assert.NotNull(models);
        Assert.Single(models);

        Assert.All(models, x => Assert.Equal(role.Id, x.Id));
        Assert.All(models, x => Assert.Equal(role.Name, x.Name));
        Assert.All(models, x => Assert.Equal(role.IsDefault, x.IsDefault));
        Assert.All(models, x => Assert.Equal(role.UpdatedDate, x.UpdatedDate));
        Assert.All(models, x => Assert.NotNull(x.PermissionIds));
        Assert.All(models, x => Assert.Single(x.PermissionIds!));
        Assert.All(models, x => Assert.Contains(rolePermission.PermissionId, x.PermissionIds!));
    }

    [Fact]
    public void SelectMultipleAsModel_ReturnsProjectedModels() {
        // arrange
        var rolePermission1 = new RolePermission { Id = 1, RoleId = 2, PermissionId = 3 };
        var role1 = new Role { Id = 4, Name = "test role 1", IsDefault = true, RolePermissions = [rolePermission1], UpdatedDate = DateTime.MinValue };
        var rolePermission2 = new RolePermission { Id = 5, RoleId = 6, PermissionId = 7 };
        var role2 = new Role { Id = 6, Name = "test role 2", IsDefault = false, RolePermissions = [rolePermission2], UpdatedDate = DateTime.MaxValue };
        var roles = new List<Role> { role1, role2 }.AsQueryable();

        // act
        var models = roles.SelectAsModel().ToList();

        // assert
        Assert.NotNull(models);
        Assert.Equal(2, models.Count);

        Assert.Collection(models,
            x => Assert.Equal(role1.Id, x.Id),
            x => Assert.Equal(role2.Id, x.Id)
        );
        Assert.Collection(models,
            x => Assert.Equal(role1.Name, x.Name),
            x => Assert.Equal(role2.Name, x.Name)
        );
        Assert.Collection(models,
            x => Assert.Equal(role1.IsDefault, x.IsDefault),
            x => Assert.Equal(role2.IsDefault, x.IsDefault)
        );
        Assert.Collection(models,
            x => Assert.Equal(role1.UpdatedDate, x.UpdatedDate),
            x => Assert.Equal(role2.UpdatedDate, x.UpdatedDate)
        );
        Assert.All(models, x => Assert.NotNull(x.PermissionIds));
        Assert.All(models, x => Assert.Single(x.PermissionIds!));

        Assert.Collection(models,
            x => Assert.Contains(rolePermission1.PermissionId, x.PermissionIds!),
            x => Assert.Contains(rolePermission2.PermissionId, x.PermissionIds!)
        );
    }
}
