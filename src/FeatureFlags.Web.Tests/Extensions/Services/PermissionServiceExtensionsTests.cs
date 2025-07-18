using FeatureFlags.Domain.Models;
using FeatureFlags.Extensions.Services;

namespace FeatureFlags.Web.Tests.Extensions.Services;

public class PermissionServiceExtensionsTests {
    [Fact]
    public void SelectSingleAsModel_ReturnsProjectedModel() {
        // arrange
        var permission = new Permission { Id = 1, ControllerName = "controller", ActionName = "action" };
        var permissions = new List<Permission> { permission }.AsQueryable();

        // act
        var models = permissions.SelectAsModel();

        // assert
        Assert.NotNull(models);
        Assert.Single(models);

        Assert.All(models, x => Assert.Equal(permission.Id, x.Id));
        Assert.All(models, x => Assert.Equal(permission.ControllerName, x.ControllerName));
        Assert.All(models, x => Assert.Equal(permission.ActionName, x.ActionName));
    }

    [Fact]
    public void SelectMultipleAsModel_ReturnsProjectedModels() {
        // arrange
        var permission1 = new Permission { Id = 1, ControllerName = "controller 1", ActionName = "action 1" };
        var permission2 = new Permission { Id = 2, ControllerName = "controller 2", ActionName = "action 2" };
        var roles = new List<Permission> { permission1, permission2 }.AsQueryable();

        // act
        var models = roles.SelectAsModel().ToList();

        // assert
        Assert.NotNull(models);
        Assert.Equal(2, models.Count);

        Assert.Collection(models,
            x => Assert.Equal(permission1.Id, x.Id),
            x => Assert.Equal(permission2.Id, x.Id)
        );
        Assert.Collection(models,
            x => Assert.Equal(permission1.ControllerName, x.ControllerName),
            x => Assert.Equal(permission2.ControllerName, x.ControllerName)
        );
        Assert.Collection(models,
            x => Assert.Equal(permission1.ActionName, x.ActionName),
            x => Assert.Equal(permission2.ActionName, x.ActionName)
        );
    }
}
