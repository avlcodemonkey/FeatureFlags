using System.Security.Claims;
using FeatureFlags.Models;

namespace FeatureFlags.Services;

public interface IUserService {
    Task<bool> DeleteUserAsync(int id, CancellationToken cancellationToken = default);

    Task<IEnumerable<UserModel>> GetAllUsersAsync(CancellationToken cancellationToken = default);

    Task<IEnumerable<Claim>> GetClaimsByUserIdAsync(int id, CancellationToken cancellationToken = default);

    Task<UserModel?> GetUserByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<UserModel?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<(bool success, string message)> SaveUserAsync(UserModel userModel, CancellationToken cancellationToken = default);

    Task<(bool success, string message)> UpdateAccountAsync(UpdateAccountModel updateAccountModel, CancellationToken cancellationToken = default);

    Task<IEnumerable<AutocompleteUserModel>> FindAutocompleteUsersByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes any existing tokens for the user and creates a new one.
    /// </summary>
    /// <param name="id">ID of user to create token for.</param>
    /// <param name="cancellationToken">Cancellation token from user request.</param>
    /// <returns>New token value.</returns>
    Task<(bool success, string? token)> CreateUserTokenAsync(int id, CancellationToken cancellationToken = default);

    Task<(bool success, string message)> VerifyUserTokenAsync(string email, string token, CancellationToken cancellationToken = default);
}
