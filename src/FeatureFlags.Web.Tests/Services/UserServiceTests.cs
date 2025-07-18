using System.Security.Claims;
using System.Security.Principal;
using FeatureFlags.Constants;
using FeatureFlags.Domain.Models;
using FeatureFlags.Models;
using FeatureFlags.Resources;
using FeatureFlags.Services;
using FeatureFlags.Utils;
using FeatureFlags.Web.Tests.Fixtures;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace FeatureFlags.Web.Tests.Services;

[Collection(nameof(DatabaseCollection))]
public class UserServiceTests {
    private readonly DatabaseFixture _Fixture;
    private readonly Mock<IHttpContextAccessor> _MockHttpContextAccessor;
    private readonly UserService _UserService;

    private UserService GetNewUserService() => new(_Fixture.CreateContext(), _MockHttpContextAccessor.Object);

    /// <summary>
    /// Delete a user so it doesn't interfere with other tests.
    /// </summary>
    private async Task DeleteUserIfExistsAsync(int userId) {
        var user = (await GetNewUserService().GetAllUsersAsync()).FirstOrDefault(x => x.Id == userId);
        if (user != null) {
            // need a new context for this to avoid concurrency error
            await GetNewUserService().DeleteUserAsync(user.Id);
        }
    }

    public UserServiceTests(DatabaseFixture fixture) {
        _Fixture = fixture;

        _MockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _MockHttpContextAccessor.Setup(x => x.HttpContext!.User.Identity).Returns(new GenericIdentity(fixture.TestUser.Email, "test"));

        _UserService = GetNewUserService();
    }

    [Fact]
    public async Task GetUserByIdAsync_ReturnsTestUserModel() {
        // arrange
        var testUser = _Fixture.TestUser;

        // act
        var user = await _UserService.GetUserByIdAsync(testUser.Id);

        // assert
        Assert.NotNull(user);
        Assert.IsType<UserModel>(user);
        Assert.Equal(testUser.Name, user.Name);
        Assert.Equal(testUser.Email, user.Email);
        Assert.Equal(testUser.LanguageId, user.LanguageId);

        Assert.NotNull(user.RoleIds);
        Assert.Equal(testUser.UserRoles.Count(), user.RoleIds.Count());
        Assert.Equal(testUser.UserRoles.First().RoleId, user.RoleIds.First());
    }

    [Fact]
    public async Task GetUserByIdAsync_WithInvalidUserId_ReturnsNull() {
        // arrange
        var userIdToGet = -200;

        // act
        var result = await _UserService.GetUserByIdAsync(userIdToGet);

        // assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserByUserEmailAsync_ReturnsTestUserModel() {
        // arrange
        var testUser = _Fixture.TestUser;

        // act
        var user = await _UserService.GetUserByEmailAsync(testUser.Email);

        // assert
        Assert.NotNull(user);
        Assert.IsType<UserModel>(user);
        Assert.Equal(testUser.Name, user.Name);
        Assert.Equal(testUser.Email, user.Email);
        Assert.Equal(testUser.LanguageId, user.LanguageId);

        Assert.NotNull(user.RoleIds);
        Assert.Equal(testUser.UserRoles.Count(), user.RoleIds.Count());
        Assert.Equal(testUser.UserRoles.First().RoleId, user.RoleIds.First());
    }

    [Fact]
    public async Task GetUserByEmailAsync_WithInvalidEmail_ReturnsNull() {
        // arrange
        var userEmailToGet = "fakeUser@google.com";

        // act
        var result = await _UserService.GetUserByEmailAsync(userEmailToGet);

        // assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllUsersAsync_ReturnsUserModels() {
        // arrange
        var user = _Fixture.User;
        var testUser = _Fixture.TestUser;

        // act
        var users = await _UserService.GetAllUsersAsync();

        // assert
        Assert.NotEmpty(users);
        Assert.IsType<IEnumerable<UserModel>>(users, exactMatch: false);
        Assert.Contains(users, x => x.Id == user.Id);
        Assert.Contains(users, x => x.Id == testUser.Id);
    }

    [Fact]
    public async Task SaveUserAsync_CreateUser_SavesNewUser() {
        // arrange
        var testRole = _Fixture.TestRole;
        var createUser = new UserModel {
            Name = "create first", Email = "create@email.com", LanguageId = 1, RoleIds = [testRole.Id]
        };

        // act
        (var success, var message) = await _UserService.SaveUserAsync(createUser);
        var newUser = (await _UserService.GetAllUsersAsync()).FirstOrDefault(x => x.Email == createUser.Email);
        if (newUser != null) {
            // delete the newly created user so it doesn't interfere with other tests
            await GetNewUserService().DeleteUserAsync(newUser.Id);
        }

        // assert
        Assert.True(success);
        Assert.Equal(Users.SuccessSavingUser, message);
        Assert.NotNull(newUser);
        Assert.NotNull(newUser.RoleIds);
        Assert.Equal(createUser.RoleIds.Count(), newUser.RoleIds.Count());
    }

    [Fact]
    public async Task SaveUserAsync_UpdateUser_SavesChanges() {
        // arrange
        var testRole = _Fixture.TestRole;
        var originalEmail = "originalEmail@a.com";
        var createUser = new UserModel {
            Name = "original first", Email = originalEmail, LanguageId = 1, RoleIds = [testRole.Id], UpdatedDate = DateTime.UtcNow
        };
        await _UserService.SaveUserAsync(createUser);

        var userId = (await _UserService.GetAllUsersAsync()).FirstOrDefault(x => x.Email == originalEmail)!.Id;
        var newEmail = "newEmail@a.com";
        var updateUser = new UserModel {
            Id = userId, Name = "new first", Email = newEmail, LanguageId = 1, RoleIds = [], UpdatedDate = DateTime.UtcNow
        };

        // act
        (var success, var message) = await _UserService.SaveUserAsync(updateUser);
        var updatedUser = (await _UserService.GetAllUsersAsync()).FirstOrDefault(x => x.Id == userId);
        if (updatedUser != null) {
            // delete the newly created user so it doesn't interfere with other tests
            await GetNewUserService().DeleteUserAsync(updatedUser.Id);
        }

        // assert
        Assert.True(success);
        Assert.Equal(Users.SuccessSavingUser, message);
        Assert.NotNull(updatedUser);
        Assert.Equal(newEmail, updatedUser.Email);
        Assert.NotEqual(createUser.Name, updatedUser.Name);
        Assert.Equal(createUser.LanguageId, updatedUser.LanguageId);
        Assert.NotNull(updatedUser.RoleIds);
        Assert.Empty(updatedUser.RoleIds);
    }

    [Fact]
    public async Task SaveUserAsync_UpdateInvalidUserId_ReturnsFalse() {
        // arrange
        var updateUser = new UserModel {
            Id = 999, Name = "update first", Email = "update@email.com",
            LanguageId = 1, RoleIds = []
        };

        // act
        (var success, var message) = await _UserService.SaveUserAsync(updateUser);

        // assert
        Assert.False(success);
        Assert.Equal(Core.ErrorInvalidId, message);
    }

    [Fact]
    public async Task SaveUserAsync_CreateWithDuplicateEmail_ReturnsDuplicateError() {
        // arrange
        var testRole = _Fixture.TestRole;
        var duplicateUser = new UserModel {
            Name = "duplicate1", Email = _Fixture.User.Email,
            LanguageId = 1, RoleIds = [testRole.Id]
        };

        // act
        (var success, var message) = await _UserService.SaveUserAsync(duplicateUser);

        // assert
        Assert.False(success);
        Assert.Equal(Users.ErrorDuplicateEmail, message);
    }

    [Fact]
    public async Task SaveUserAsync_UpdateWithDuplicateEmail_ReturnsDuplicateError() {
        // arrange
        var testRole = _Fixture.TestRole;
        var originalEmail = "duplicate2@aaa.com";
        var duplicateUser = new UserModel {
            Name = "duplicate2", Email = originalEmail,
            LanguageId = 1, RoleIds = [testRole.Id]
        };
        await _UserService.SaveUserAsync(duplicateUser);

        var userCopy = (await _UserService.GetAllUsersAsync()).FirstOrDefault(x => x.Email == originalEmail);
        var updateUser = userCopy! with { Email = _Fixture.User.Email };

        // act
        (var success, var message) = await _UserService.SaveUserAsync(updateUser);

        // assert
        Assert.False(success);
        Assert.Equal(Users.ErrorDuplicateEmail, message);
    }

    [Fact]
    public async Task SaveUserAsync_WithConcurrentChanges_ReturnsConcurrencyError() {
        // arrange
        var testRole = _Fixture.TestRole;
        var originalEmail = "concurrency1@aaa.com";
        var createUser = new UserModel {
            Name = "concurrency1 first", Email = originalEmail,
            LanguageId = 1, RoleIds = [testRole.Id]
        };
        await _UserService.SaveUserAsync(createUser);

        var userCopy = (await _UserService.GetAllUsersAsync()).FirstOrDefault(x => x.Email == originalEmail);
        // decrement UpdatedDate to emulate an older row version
        var updateUser = userCopy! with { Email = "concurrency1@bbb.com", UpdatedDate = userCopy.UpdatedDate.AddSeconds(-10) };

        // act
        (var success, var message) = await _UserService.SaveUserAsync(updateUser);

        await DeleteUserIfExistsAsync(userCopy.Id);

        // assert
        Assert.False(success);
        Assert.Equal(Core.ErrorConcurrency, message);
    }

    [Fact]
    public async Task SaveUserAsync_WithSameUpdatedDateAndConcurrentChanges_ReturnsConcurrencyError() {
        // arrange
        var testRole = _Fixture.TestRole;
        var originalEmail = "concurrency2@aaa.com";
        var createUser = new UserModel {
            Name = "concurrency2 first", Email = originalEmail,
            LanguageId = 1, RoleIds = [testRole.Id]
        };
        await _UserService.SaveUserAsync(createUser);

        var userCopy = (await _UserService.GetAllUsersAsync()).First(x => x.Email == originalEmail);
        var updateUser = userCopy with { Email = "concurrency2@bbb.com" };
        var finalUser = userCopy with { Email = "concurrency2@ccc.com" };
        await Task.Delay(1100);

        // act
        (var success, var message) = await GetNewUserService().SaveUserAsync(updateUser);
        (var success2, var message2) = await GetNewUserService().SaveUserAsync(finalUser);

        await DeleteUserIfExistsAsync(userCopy.Id);

        // assert
        Assert.True(success);
        Assert.False(success2);
        Assert.Equal(Users.SuccessSavingUser, message);
        Assert.Equal(Core.ErrorConcurrency, message2);
    }

    [Fact]
    public async Task SaveUserAsync_WithIncrementedUpdatedDateAndConcurrentChanges_ReturnsSuccess() {
        // arrange
        var testRole = _Fixture.TestRole;
        var originalEmail = "concurrency3@aaa.com";
        var createUser = new UserModel {
            Name = "concurrency3 first", Email = originalEmail,
            LanguageId = 1, RoleIds = [testRole.Id]
        };
        await _UserService.SaveUserAsync(createUser);

        var userCopy = (await _UserService.GetAllUsersAsync()).FirstOrDefault(x => x.Email == originalEmail);
        var updateUser = userCopy! with { Email = "concurrency3@bbb.com" };
        var finalUser = userCopy with { Email = "concurrency3@ccc.com", UpdatedDate = userCopy.UpdatedDate.AddSeconds(10) };

        // act
        (var success, var message) = await _UserService.SaveUserAsync(updateUser);
        (var success2, var message2) = await _UserService.SaveUserAsync(finalUser);

        await DeleteUserIfExistsAsync(userCopy.Id);

        // assert
        Assert.True(success);
        Assert.True(success2);
        Assert.Equal(Users.SuccessSavingUser, message);
        Assert.Equal(Users.SuccessSavingUser, message2);
    }

    [Fact]
    public async Task DeleteUserAsync_WithValidUser_DeletesUser() {
        // arrange
        var userToDelete = new User {
            Id = -100, Name = "delete first", Email = "delete@email.com", LanguageId = 1, Status = true
        };
        using (var dbContext = _Fixture.CreateContext()) {
            dbContext.Users.Add(userToDelete);
            await dbContext.SaveChangesAsync();
        }

        // act
        var result = await _UserService.DeleteUserAsync(userToDelete.Id);
        var deletedUser = await _Fixture.CreateContext().Users.FirstOrDefaultAsync(x => x.Id == userToDelete.Id);

        // assert
        Assert.True(result);
        Assert.NotNull(deletedUser);
        Assert.False(deletedUser.Status);
    }

    [Fact]
    public async Task DeleteUserAsync_WithInvalidUserId_ReturnsFalse() {
        // arrange
        var userIdToDelete = -200;

        // act
        var result = await _UserService.DeleteUserAsync(userIdToDelete);

        // assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetClaimsByUserIdAsync_WithValidUserId_ReturnsUserClaims() {
        // arrange
        var testUser = _Fixture.TestUser;
        var testPermission = _Fixture.TestPermission;

        // act
        var claims = await _UserService.GetClaimsByUserIdAsync(testUser.Id);

        // assert
        Assert.NotEmpty(claims);
        Assert.IsType<IEnumerable<Claim>>(claims, exactMatch: false);
        var x = Assert.Single(claims);
        Assert.Equal($"{testPermission.ControllerName}.{testPermission.ActionName}", x.Value);
    }

    [Fact]
    public async Task GetClaimsByUserIdAsync_WithInvalidUserId_ReturnsEmptyList() {
        // arrange
        var userId = -300;

        // act
        var claims = await _UserService.GetClaimsByUserIdAsync(userId);

        // assert
        Assert.Empty(claims);
        Assert.IsType<IEnumerable<Claim>>(claims, exactMatch: false);
    }

    [Fact]
    public async Task UpdateAccountAsync_WithValidEmail_ReturnsSuccess() {
        // arrange
        var testRole = _Fixture.TestRole;

        // first create a new user in the db
        var originalEmail = "updateAccountValid@aaa.com";
        var originalName = "update account valid";
        var originalLanguageId = 1;
        var updateAccountUser = new UserModel {
            Name = originalName, Email = originalEmail, LanguageId = originalLanguageId, RoleIds = [testRole.Id]
        };
        await _UserService.SaveUserAsync(updateAccountUser);

        // now load the new user from the db so we have their userId
        var user = await _Fixture.CreateContext().Users.FirstOrDefaultAsync(x => x.Email == originalEmail);

        // now create the UpdateAccount record
        var newName = "update account new";
        var newLanguageId = 2;
        var updateAccount = new UpdateAccountModel {
            Email = originalEmail, Name = newName, LanguageId = newLanguageId
        };

        // create a new userService that will act like the new user
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        mockHttpContextAccessor.Setup(x => x.HttpContext!.User.Identity).Returns(new GenericIdentity(originalEmail, "test"));
        var userService = new UserService(_Fixture.CreateContext(user), mockHttpContextAccessor.Object);

        // act
        (var success, var message) = await userService.UpdateAccountAsync(updateAccount);
        var updatedUser = (await _UserService.GetAllUsersAsync()).FirstOrDefault(x => x.Id == user!.Id);
        if (updatedUser != null) {
            // delete the newly created user so it doesn't interfere with other tests
            // need a new context for this to avoid concurrency error
            await GetNewUserService().DeleteUserAsync(updatedUser.Id);
        }

        // assert
        Assert.True(success);
        Assert.Equal(Account.AccountUpdated, message);
        Assert.NotNull(updatedUser);
        Assert.Equal(newName, updatedUser.Name);
        Assert.Equal(originalEmail, updatedUser.Email);
        Assert.Equal(newLanguageId, updatedUser.LanguageId);
    }

    [Fact]
    public async Task UpdateAccountAsync_WithNoIdentity_ReturnsError() {
        // arrange
        var testRole = _Fixture.TestRole;

        // first create a new user in the db
        var originalEmail = "updateAccountNoIdentity@aaa.com";
        var originalName = "update account no identity";
        var originalLanguageId = 1;
        var updateAccountUser = new UserModel {
            Name = originalName, Email = originalEmail, LanguageId = originalLanguageId, RoleIds = [testRole.Id]
        };
        await _UserService.SaveUserAsync(updateAccountUser);

        // now create the UpdateAccount record
        var updateAccount = new UpdateAccountModel {
            Name = originalName, LanguageId = originalLanguageId
        };

        // create a new userService that will act like the new user
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var userService = new UserService(_Fixture.CreateContext(), mockHttpContextAccessor.Object);

        // act
        (var success, var message) = await userService.UpdateAccountAsync(updateAccount);

        // assert
        Assert.False(success);
        Assert.Equal(Core.ErrorGeneric, message);
    }

    [Fact]
    public async Task UpdateAccountAsync_WithInvalidEmail_ReturnsError() {
        // arrange
        var testRole = _Fixture.TestRole;

        // first create a new user in the db
        var originalEmail = "updateAccountInvalid@aaa.com";
        var originalName = "update account invalid";
        var originalLanguageId = 1;
        var updateAccountUser = new UserModel {
            Name = originalName, Email = originalEmail, LanguageId = originalLanguageId, RoleIds = [testRole.Id]
        };
        await _UserService.SaveUserAsync(updateAccountUser);

        // now load the new user from the db so we have their userId
        var user = await _Fixture.CreateContext().Users.FirstOrDefaultAsync(x => x.Email == originalEmail);

        // now create the UpdateAccount record
        var badEmail = "updateAccountNewEmail@aaa.com";
        var newName = "update account new";
        var newLanguageId = 2;
        var updateAccount = new UpdateAccountModel {
            Email = originalEmail, Name = newName, LanguageId = newLanguageId
        };

        // create a new userService that will act like the new user
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        mockHttpContextAccessor.Setup(x => x.HttpContext!.User.Identity).Returns(new GenericIdentity(badEmail, "test"));
        var userService = new UserService(_Fixture.CreateContext(user), mockHttpContextAccessor.Object);

        // act
        (var success, var message) = await userService.UpdateAccountAsync(updateAccount);
        var updatedUser = (await _UserService.GetAllUsersAsync()).FirstOrDefault(x => x.Id == user!.Id);
        if (updatedUser != null) {
            // delete the newly created user so it doesn't interfere with other tests
            // need a new context for this to avoid concurrency error
            await GetNewUserService().DeleteUserAsync(updatedUser.Id);
        }

        // assert
        Assert.False(success);
        Assert.Equal(Core.ErrorInvalidId, message);
        Assert.NotNull(updatedUser);
        Assert.Equal(originalName, updatedUser.Name);
        Assert.Equal(originalEmail, updatedUser.Email);
        Assert.Equal(originalLanguageId, updatedUser.LanguageId);
    }

    [Fact]
    public async Task FindAutocompleteUsersByNameAsync_WithEmptyQuery_ReturnsUserModels() {
        // arrange
        var user = _Fixture.User;
        var testUser = _Fixture.TestUser;

        // act
        var users = await _UserService.FindAutocompleteUsersByNameAsync("");

        // assert
        Assert.NotEmpty(users);
        Assert.IsType<IEnumerable<AutocompleteUserModel>>(users, exactMatch: false);
        Assert.Contains(users, x => x.Value == user.Id);
        Assert.Contains(users, x => x.Label == NameHelper.DisplayName(user.Name, user.Email));
        Assert.Contains(users, x => x.Value == testUser.Id);
        Assert.Contains(users, x => x.Label == NameHelper.DisplayName(testUser.Name, testUser.Email));
    }

    [Fact]
    public async Task FindAutocompleteUsersByNameAsync_WithUserQuery_ReturnsMatchingModel() {
        // arrange
        var user = _Fixture.User;

        // act
        var users = await _UserService.FindAutocompleteUsersByNameAsync(user.Name);

        // assert
        Assert.NotEmpty(users);
        Assert.IsType<IEnumerable<AutocompleteUserModel>>(users, exactMatch: false);
        var resultUser = Assert.Single(users);
        Assert.Equal(user.Id, resultUser.Value);
    }

    [Fact]
    public async Task FindAutocompleteUsersByNameAsync_WithGibberishQuery_ReturnsNoMatches() {
        // arrange

        // act
        var users = await _UserService.FindAutocompleteUsersByNameAsync("gibberish");

        // assert
        Assert.Empty(users);
    }


    [Fact]
    public async Task CreateUserTokenAsync_WithInvalidUser_ReturnsError() {
        // Arrange
        var userId = -9999;

        // Act
        (var success, var token) = await _UserService.CreateUserTokenAsync(userId);

        // Assert
        Assert.False(success);
        Assert.Null(token);
    }

    [Fact]
    public async Task CreateUserTokenAsync_WithNoTokenToFind_CreatesNewToken() {
        // Arrange
        var user = _Fixture.User;

        //// Act
        (var success, var token) = await _UserService.CreateUserTokenAsync(user.Id);

        //// Assert
        Assert.True(success);
        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }

    [Fact]
    public async Task CreateUserTokenAsync_WithNoExistingToken_CreatesNewToken() {
        // Arrange
        var user = new User {
            Email = "createToken@aaa.com", Name = "createToken", LanguageId = 1, Status = true
        };
        using (var dbContext = _Fixture.CreateContext()) {
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();
        }

        // Act
        (var _, var token) = await _UserService.CreateUserTokenAsync(user.Id);
        var newToken = await _Fixture.CreateContext().UserTokens.FirstOrDefaultAsync(x => x.UserId == user.Id && x.Token == token);

        // Assert
        Assert.NotNull(newToken);
        Assert.Equal(token, newToken.Token);
    }

    [Fact]
    public async Task CreateUserTokenAsync_WithExistingToken_DeletesExistingTokenAndCreatesNewToken() {
        // Arrange
        var token = "valid-token";
        var (user, _) = await CreateUserAndTokenAsync("deleteToken@aaa.com", token, DateTime.UtcNow);

        // Act
        (var success, var newToken) = await _UserService.CreateUserTokenAsync(user.Id);
        UserToken? originalToken = null;
        UserToken? newUserToken = null;
        using (var dbContext = _Fixture.CreateContext()) {
            originalToken = await dbContext.UserTokens.FirstOrDefaultAsync(x => x.UserId == user.Id && x.Token == token);
            newUserToken = await dbContext.UserTokens.FirstOrDefaultAsync(x => x.UserId == user.Id && x.Token == newToken);

            dbContext.Users.Remove(user);
            if (newUserToken != null) {
                dbContext.UserTokens.Remove(newUserToken);
            }
            await dbContext.SaveChangesAsync();
        }

        // Assert
        Assert.True(success);
        Assert.Null(originalToken);
        Assert.NotNull(newToken);
        Assert.NotEqual(token, newToken);
    }

    [Fact]
    public async Task VerifyUserTokenAsync_WithNonExistentToken_ReturnsError() {
        // Arrange
        var user = _Fixture.User;
        var token = "gibberish-token";

        // Act
        (var success, var message) = await _UserService.VerifyUserTokenAsync(user.Email, token);

        // Assert
        Assert.False(success);
        Assert.Equal(Account.ErrorInvalidToken, message);
    }

    [Fact]
    public async Task VerifyUserTokenAsync_WithValidToken_ReturnsSuccess() {
        // Arrange
        var token = "valid-token";
        var (user, userToken) = await CreateUserAndTokenAsync("verifyToken@aaa.com", token, DateTime.UtcNow.AddMinutes(Auth.TokenLifeTime));

        // Act
        (var success, var message) = await _UserService.VerifyUserTokenAsync(user.Email, token);

        // Assert
        Assert.True(success);
        Assert.Empty(message);
    }

    [Fact]
    public async Task VerifyUserTokenAsync_WithExpiredToken_ReturnsErrorAndDeletesToken() {
        // Arrange
        var token = "expired-token";
        var (user, userToken) = await CreateUserAndTokenAsync("expiredToken@aaa.com", token, DateTime.UtcNow.AddMinutes(-Auth.TokenLifeTime));

        // Act
        (var success, var message) = await _UserService.VerifyUserTokenAsync(user.Email, token);

        // Assert
        Assert.False(success);
        Assert.Equal(Account.ErrorTokenDeleted, message);
    }

    [Fact]
    public async Task VerifyUserTokenAsync_WithInvalidToken_ReturnsError() {
        // Arrange
        var token = "invalid-token";
        var (user, userToken) = await CreateUserAndTokenAsync("invalidToken@aaa.com", token, DateTime.UtcNow.AddMinutes(Auth.TokenLifeTime));

        // Act
        (var success, var message) = await _UserService.VerifyUserTokenAsync(user.Email, "gibberish");
        using (var dbContext = _Fixture.CreateContext()) {
            dbContext.UserTokens.Remove(userToken);
            await dbContext.SaveChangesAsync();
        }

        // Assert
        Assert.False(success);
        Assert.Equal(Account.ErrorInvalidToken, message);
    }

    [Fact]
    public async Task VerifyUserTokenAsync_WithInvalidEmail_ReturnsError() {
        // Arrange
        var token = "valid-token";
        var (_, userToken) = await CreateUserAndTokenAsync("validToken@aaa.com", token, DateTime.UtcNow.AddMinutes(Auth.TokenLifeTime));

        // Act
        (var success, var message) = await _UserService.VerifyUserTokenAsync("random-email@domain.com", token);
        using (var dbContext = _Fixture.CreateContext()) {
            dbContext.UserTokens.Remove(userToken);
            await dbContext.SaveChangesAsync();
        }

        // Assert
        Assert.False(success);
        Assert.Equal(Core.ErrorGeneric, message);
    }

    [Fact]
    public async Task VerifyUserTokenAsync_WithInvalidTokenAndOneAttemptLeft_ReturnsErrorAndDeletesToken() {
        // Arrange
        var token = "valid-token";
        var (user, userToken) = await CreateUserAndTokenAsync("fourAttempts@aaa.com", token, DateTime.UtcNow.AddMinutes(Auth.TokenLifeTime), Auth.TokenMaxAttempts - 1);

        // Act
        (var success, var message) = await _UserService.VerifyUserTokenAsync(user.Email, "gibberish");
        var updatedToken = await GetTokenAndDeleteAsync(userToken.UserId, token);

        // Assert
        Assert.False(success);
        Assert.Equal(Account.ErrorTokenDeleted, message);
        Assert.Null(updatedToken);
    }

    [Fact]
    public async Task VerifyUserTokenAsync_WithInvalidTokenAndTooManyAttempts_ReturnsErrorAndDeletesToken() {
        // Arrange
        var token = "invalid-token";
        var (user, userToken) = await CreateUserAndTokenAsync("tooManyAttempts@aaa.com", token, DateTime.UtcNow.AddMinutes(Auth.TokenLifeTime), Auth.TokenMaxAttempts);

        // Act
        (var success, var message) = await _UserService.VerifyUserTokenAsync(user.Email, "gibberish");
        var updatedToken = await GetTokenAndDeleteAsync(userToken.UserId, token);

        // Assert
        Assert.False(success);
        Assert.Equal(Account.ErrorTokenDeleted, message);
        Assert.Null(updatedToken);
    }

    [Fact]
    public async Task VerifyUserTokenAsync_WithValidTokenAndOneAttemptLeft_ReturnsUserAndDeletesToken() {
        // Arrange
        var token = "valid-token";
        var (user, userToken) = await CreateUserAndTokenAsync("validFour@aaa.com", token, DateTime.UtcNow.AddMinutes(Auth.TokenLifeTime), Auth.TokenMaxAttempts - 1);

        // Act
        (var success, var message) = await _UserService.VerifyUserTokenAsync(user.Email, token);
        var updatedToken = await GetTokenAndDeleteAsync(userToken.UserId, token);

        // Assert
        Assert.True(success);
        Assert.Equal("", message);
        Assert.Null(updatedToken);
    }

    private async Task<(User user, UserToken userToken)> CreateUserAndTokenAsync(string email, string token, DateTime? expirationDate, int? attempts = null) {
        var user = new User {
            Email = email, Name = email.Split('@')[0], LanguageId = 1, Status = true
        };
        var userToken = new UserToken {
            Token = token,
            ExpirationDate = expirationDate,
            Attempts = attempts ?? 0
        };
        using (var dbContext = _Fixture.CreateContext()) {
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();

            userToken.UserId = user.Id;
            dbContext.UserTokens.Add(userToken);
            await dbContext.SaveChangesAsync();
        }
        return (user, userToken);
    }

    private async Task<UserToken?> GetTokenAndDeleteAsync(int userId, string token) {
        UserToken? userToken = null;
        using (var dbContext = _Fixture.CreateContext()) {
            userToken = await dbContext.UserTokens.FirstOrDefaultAsync(x => x.UserId == userId && x.Token == token);
            if (userToken != null) {
                dbContext.UserTokens.Remove(userToken);
                await dbContext.SaveChangesAsync();
            }
        }
        return userToken;
    }
}
