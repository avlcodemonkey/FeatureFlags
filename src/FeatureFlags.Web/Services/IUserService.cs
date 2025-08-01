using System.Security.Claims;
using FeatureFlags.Models;

namespace FeatureFlags.Services;

/// <summary>
/// Provides user management operations for the application.
/// </summary>
public interface IUserService {
    /// <summary>
    /// Deletes a user by unique identifier.
    /// </summary>
    /// <param name="id">Unique identifier of user to delete.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>True if user was deleted; otherwise, false.</returns>
    Task<bool> DeleteUserAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all users in the system.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>Collection of user models.</returns>
    Task<IEnumerable<UserModel>> GetAllUsersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves claims associated with a user by unique identifier.
    /// </summary>
    /// <param name="id">Unique identifier of user.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>Collection of claims for user.</returns>
    Task<IEnumerable<Claim>> GetClaimsByUserIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a user by unique identifier.
    /// </summary>
    /// <param name="id">Unique identifier of user.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>User model if found; otherwise, null.</returns>
    Task<UserModel?> GetUserByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a user by email address.
    /// </summary>
    /// <param name="email">Email address of user.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>User model if found; otherwise, null.</returns>
    Task<UserModel?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves a user to the system.
    /// </summary>
    /// <param name="userModel">User model to save.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>Tuple indicating success and message.</returns>
    Task<(bool success, string message)> SaveUserAsync(UserModel userModel, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates account information for a user.
    /// </summary>
    /// <param name="updateAccountModel">Model containing updated account information.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>Tuple indicating success and message.</returns>
    Task<(bool success, string message)> UpdateAccountAsync(UpdateAccountModel updateAccountModel, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds users by name for autocomplete functionality.
    /// </summary>
    /// <param name="name">Name to search for.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>Collection of autocomplete user models.</returns>
    Task<IEnumerable<AutocompleteUserModel>> FindAutocompleteUsersByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes any existing tokens for user and creates a new one.
    /// </summary>
    /// <param name="id">ID of user to create token for.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>Tuple indicating success and new token value.</returns>
    Task<(bool success, string? token)> CreateUserTokenAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies a user token for login or authentication.
    /// </summary>
    /// <param name="email">Email address of user.</param>
    /// <param name="token">Token to verify.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>Tuple indicating success and message.</returns>
    Task<(bool success, string message)> VerifyUserTokenAsync(string email, string token, CancellationToken cancellationToken = default);
}
