using FeatureFlags.Domain.Models;
using FeatureFlags.Extensions.Services;
using FeatureFlags.Utils;

namespace FeatureFlags.Web.Tests.Extensions.Services;

public class UserServiceExtensionsTests {
    [Fact]
    public void SelectSingleAsModel_ReturnsProjectedModel() {
        // arrange
        var userRole = new UserRole { Id = 1, RoleId = 2, UserId = 3 };
        var user = new User {
            Id = 3, Email = "fake@email.com", Name = "name", LanguageId = 1,
            UserRoles = new List<UserRole> { userRole }, UpdatedDate = DateTime.MaxValue
        };
        var users = new List<User> { user }.AsQueryable();

        // act
        var models = users.SelectAsModel();

        // assert
        Assert.NotNull(models);
        var resultUser = Assert.Single(models);
        Assert.Equal(user.Id, resultUser.Id);
        Assert.Equal(user.Email, resultUser.Email);
        Assert.Equal(user.Name, resultUser.Name);
        Assert.Equal(user.LanguageId, resultUser.LanguageId);
        Assert.Equal(user.UpdatedDate, resultUser.UpdatedDate);

        Assert.NotNull(resultUser.RoleIds);
        Assert.Single(resultUser.RoleIds!);
        Assert.Contains(userRole.RoleId, resultUser.RoleIds!);
    }

    [Fact]
    public void SelectMultipleAsModel_ReturnsProjectedModels() {
        // arrange
        var userRole1 = new UserRole { Id = 1, RoleId = 2, UserId = 3 };
        var user1 = new User {
            Id = 3, Email = "fake@email.com", Name = "name", LanguageId = 1,
            UserRoles = new List<UserRole> { userRole1 }, UpdatedDate = DateTime.MinValue
        };
        var userRole2 = new UserRole { Id = 4, RoleId = 5, UserId = 6 };
        var user2 = new User {
            Id = 6, Email = "fake2@email.com", Name = "name 2", LanguageId = 2,
            UserRoles = new List<UserRole> { userRole2 }, UpdatedDate = DateTime.MaxValue
        };
        var users = new List<User> { user1, user2 }.AsQueryable();


        // act
        var models = users.SelectAsModel().ToList();

        // assert
        Assert.NotNull(models);
        Assert.Equal(2, models.Count);

        Assert.Collection(models,
            x => Assert.Equal(user1.Id, x.Id),
            x => Assert.Equal(user2.Id, x.Id)
        );
        Assert.Collection(models,
            x => Assert.Equal(user1.Email, x.Email),
            x => Assert.Equal(user2.Email, x.Email)
        );
        Assert.Collection(models,
            x => Assert.Equal(user1.Name, x.Name),
            x => Assert.Equal(user2.Name, x.Name)
        );
        Assert.Collection(models,
            x => Assert.Equal(user1.LanguageId, x.LanguageId),
            x => Assert.Equal(user2.LanguageId, x.LanguageId)
        );
        Assert.Collection(models,
            x => Assert.Equal(user1.UpdatedDate, x.UpdatedDate),
            x => Assert.Equal(user2.UpdatedDate, x.UpdatedDate)
        );

        Assert.All(models, x => Assert.NotNull(x.RoleIds));
        Assert.All(models, x => Assert.Single(x.RoleIds!));
        Assert.Collection(models,
            x => Assert.Contains(userRole1.RoleId, x.RoleIds!),
            x => Assert.Contains(userRole2.RoleId, x.RoleIds!)
        );
    }

    [Fact]
    public void SelectSingleAsAuditLogUserModel_ReturnsProjectedModel() {
        // arrange
        var userRole = new UserRole { Id = 1, RoleId = 2, UserId = 3 };
        var user = new User {
            Id = 3, Email = "fake@email.com", Name = "name", LanguageId = 1,
            UserRoles = new List<UserRole> { userRole }
        };
        var users = new List<User> { user }.AsQueryable();

        // act
        var models = users.SelectAsAuditLogUserModel();

        // assert
        Assert.NotNull(models);
        var resultUser = Assert.Single(models);
        Assert.Equal(user.Id, resultUser.Value);
        Assert.Equal(NameHelper.DisplayName(user.Name, user.Email), resultUser.Label);
    }

    [Fact]
    public void SelectMultipleAsAuditLogUserModel_ReturnsProjectedModels() {
        // arrange
        var userRole1 = new UserRole { Id = 1, RoleId = 2, UserId = 3 };
        var user1 = new User {
            Id = 3, Email = "fake@email.com", Name = "name", LanguageId = 1,
            UserRoles = new List<UserRole> { userRole1 }
        };
        var userRole2 = new UserRole { Id = 4, RoleId = 5, UserId = 6 };
        var user2 = new User {
            Id = 6, Email = "fake2@email.com", Name = "name 2", LanguageId = 2,
            UserRoles = new List<UserRole> { userRole2 }
        };
        var users = new List<User> { user1, user2 }.AsQueryable();

        // act
        var models = users.SelectAsAuditLogUserModel().ToList();

        // assert
        Assert.NotNull(models);
        Assert.Equal(2, models.Count);

        Assert.Collection(models,
            x => Assert.Equal(user1.Id, x.Value),
            x => Assert.Equal(user2.Id, x.Value)
        );
        Assert.Collection(models,
            x => Assert.Equal(NameHelper.DisplayName(user1.Name, user1.Email), x.Label),
            x => Assert.Equal(NameHelper.DisplayName(user2.Name, user2.Email), x.Label)
        );
    }
}
