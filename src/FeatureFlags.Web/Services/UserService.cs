using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Security.Claims;
using FeatureFlags.Constants;
using FeatureFlags.Domain;
using FeatureFlags.Domain.Models;
using FeatureFlags.Extensions.Services;
using FeatureFlags.Models;
using FeatureFlags.Resources;
using FeatureFlags.Utils;
using Microsoft.EntityFrameworkCore;

namespace FeatureFlags.Services;

public sealed class UserService(FeatureFlagsDbContext dbContext, IHttpContextAccessor httpContextAccessor) : IUserService {
    private readonly FeatureFlagsDbContext _DbContext = dbContext;
    private readonly IHttpContextAccessor _HttpContextAccessor = httpContextAccessor;

    public async Task<bool> DeleteUserAsync(int id, CancellationToken cancellationToken = default) {
        var user = await _DbContext.Users.FirstOrDefaultAsync(x => x.Id == id && x.Status, cancellationToken);
        if (user == null) {
            return false;
        }

        user.Status = false;
        _DbContext.Users.Update(user);
        return await _DbContext.SaveChangesAsync(cancellationToken) > 0;
    }

    public async Task<IEnumerable<UserModel>> GetAllUsersAsync(CancellationToken cancellationToken = default)
        => await _DbContext.Users.Where(x => x.Status).SelectAsModel().ToListAsync(cancellationToken);

    public async Task<IEnumerable<Claim>> GetClaimsByUserIdAsync(int id, CancellationToken cancellationToken = default)
        => await _DbContext.UserRoles
            .Include(x => x.Role)
                .ThenInclude(x => x.RolePermissions)
                    .ThenInclude(x => x.Permission)
            .Where(x => x.UserId == id)
            .SelectMany(x => x.Role.RolePermissions.Select(y => y.Permission))
            .Select(x => new Claim(ClaimTypes.Role, $"{x.ControllerName}.{x.ActionName}".ToLower(CultureInfo.InvariantCulture)))
            .ToListAsync(cancellationToken);

    // @todo why always include UserRoles?
    public async Task<UserModel?> GetUserByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _DbContext.Users.Include(x => x.UserRoles).Where(x => x.Id == id && x.Status).SelectAsModel().FirstOrDefaultAsync(cancellationToken);

    public async Task<UserModel?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
        => await _DbContext.Users.Include(x => x.UserRoles).Where(x => x.Email == email && x.Status).SelectAsModel().FirstOrDefaultAsync(cancellationToken);

    public async Task<(bool success, string message)> SaveUserAsync(UserModel userModel, CancellationToken cancellationToken = default) {
        if (userModel.Id > 0) {
            var user = await _DbContext.Users.Include(x => x.UserRoles).Where(x => x.Id == userModel.Id).FirstOrDefaultAsync(cancellationToken);
            if (user == null) {
                return (false, Core.ErrorInvalidId);
            }

            // prevent concurrent changes
            if (user.UpdatedDate > userModel.UpdatedDate) {
                return (false, Core.ErrorConcurrency);
            }

            // check that email is unique
            var emailUser = await _DbContext.Users.FirstOrDefaultAsync(x => x.Email == userModel.Email, cancellationToken);
            if (emailUser != null && userModel.Id != emailUser.Id) {
                return (false, Users.ErrorDuplicateEmail);
            }

            await MapToEntity(userModel, user, cancellationToken);

            _DbContext.Users.Update(user!);
        } else {
            // check that email is unique
            var emailUser = await _DbContext.Users.FirstOrDefaultAsync(x => x.Email == userModel.Email, cancellationToken);
            if (emailUser != null) {
                return (false, Users.ErrorDuplicateEmail);
            }

            var user = new User();
            await MapToEntity(userModel, user, cancellationToken);
            _DbContext.Users.Add(user);
        }

        return await _DbContext.SaveChangesAsync(cancellationToken) > 0 ? (true, Users.SuccessSavingUser) : (false, Core.ErrorGeneric);
    }

    public async Task<(bool success, string message)> UpdateAccountAsync(UpdateAccountModel updateAccountModel, CancellationToken cancellationToken = default) {
        var email = _HttpContextAccessor.HttpContext?.User?.Identity?.Name;
        if (string.IsNullOrWhiteSpace(email)) {
            return (false, Core.ErrorGeneric);
        }

        var user = await _DbContext.Users.FirstOrDefaultAsync(x => x.Email == email && x.Status, cancellationToken);
        if (user == null) {
            return (false, Core.ErrorInvalidId);
        }

        user.Name = updateAccountModel.Name;
        user.LanguageId = updateAccountModel.LanguageId;
        _DbContext.Users.Update(user);

        return (await _DbContext.SaveChangesAsync(cancellationToken) > 0) ? (true, Account.AccountUpdated) : (false, Core.ErrorGeneric);
    }

    [SuppressMessage("Performance", "CA1862:Use the 'StringComparison' method overloads to perform case-insensitive string comparisons",
        Justification = "Linq can't translate stringComparison methods to sql.")
    ]
    public async Task<IEnumerable<AutocompleteUserModel>> FindAutocompleteUsersByNameAsync(string name, CancellationToken cancellationToken = default) {
        var lowerName = (name ?? "").ToLower();
        return await _DbContext.Users
            .Where(x => x.Status && (x.Name.ToLower().Contains(lowerName) || (x.Email ?? "").ToLower().Contains(lowerName)))
            .SelectAsAuditLogUserModel().OrderBy(x => x.Value).ToListAsync(cancellationToken);
    }

    public async Task<(bool success, string? token)> CreateUserTokenAsync(int id, CancellationToken cancellationToken = default) {
        var user = await _DbContext.Users.FirstOrDefaultAsync(x => x.Id == id && x.Status, cancellationToken);
        if (user == null) {
            return (false, null);
        }

        var existingTokens = await _DbContext.UserTokens.Where(x => x.UserId == user.Id).ToListAsync(cancellationToken);
        _DbContext.UserTokens.RemoveRange(existingTokens);

        var token = KeyGenerator.GetUniqueKey(Auth.TokenSize);
        var userToken = new UserToken {
            UserId = user.Id, Token = token, ExpirationDate = DateTime.UtcNow.AddMinutes(Auth.TokenLifeTime), Attempts = 0
        };

        _DbContext.UserTokens.Add(userToken);

        return (await _DbContext.SaveChangesAsync(cancellationToken) > 0) ? (true, token) : (false, null);
    }

    public async Task<(bool success, string message)> VerifyUserTokenAsync(string email, string token, CancellationToken cancellationToken = default) {
        var userToken = await _DbContext.UserTokens.FirstOrDefaultAsync(x => x.User.Email == email && x.User.Status, cancellationToken);
        if (userToken == null) {
            return (false, Core.ErrorGeneric);
        }

        if (userToken.ExpirationDate < DateTime.UtcNow) {
            // token has expired, delete it
            _DbContext.UserTokens.Remove(userToken);
            return (false, await _DbContext.SaveChangesAsync(cancellationToken) > 0 ? Account.ErrorTokenDeleted : Core.ErrorGeneric);
        }

        if (userToken.Token != token) {
            // token is wrong
            if (userToken.Attempts + 1 >= Auth.TokenMaxAttempts) {
                // too many attempts for this user, delete token
                _DbContext.UserTokens.Remove(userToken);
                return (false, await _DbContext.SaveChangesAsync(cancellationToken) > 0 ? Account.ErrorTokenDeleted : Core.ErrorGeneric);
            }

            // increment attempt count
            userToken.Attempts += 1;
            _DbContext.UserTokens.Update(userToken);
            return (false, await _DbContext.SaveChangesAsync(cancellationToken) > 0 ? Account.ErrorInvalidToken : Core.ErrorGeneric);
        }

        // valid token, delete it
        _DbContext.UserTokens.Remove(userToken);
        return await _DbContext.SaveChangesAsync(cancellationToken) > 0 ? (true, "") : (false, Core.ErrorGeneric);
    }

    /// <summary>
    /// Maps data from the user model onto the user entity.
    /// </summary>
    private async Task MapToEntity(UserModel userModel, User user, CancellationToken cancellationToken = default) {
        user.Email = userModel.Email;
        user.Name = userModel.Name;
        user.LanguageId = userModel.LanguageId;
        user.Status = true;

        var existingUserRoles = new Dictionary<int, UserRole>();
        if (user.Id > 0 && userModel.RoleIds?.Any() == true) {
            existingUserRoles = (await _DbContext.UserRoles.Where(x => x.UserId == user.Id).ToListAsync(cancellationToken)).ToDictionary(x => x.RoleId, x => x);
        }

        user.UserRoles = userModel.RoleIds?.Select(x => {
            if (existingUserRoles.TryGetValue(x, out var userRole)) {
                return userRole;
            }
            return new UserRole { RoleId = x, UserId = userModel.Id };
        }).ToList() ?? [];
    }
}
