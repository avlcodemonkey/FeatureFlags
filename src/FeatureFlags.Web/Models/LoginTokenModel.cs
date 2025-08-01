using System.ComponentModel.DataAnnotations;
using FeatureFlags.Resources;
using FeatureFlags.Validators;

namespace FeatureFlags.Models;

/// <summary>
/// Model representing login using a token sent to the user's email.
/// </summary>
public sealed record LoginTokenModel {
    /// <summary>
    /// Gets the email address of the user.
    /// </summary>
    [Display(ResourceType = typeof(Users), Name = nameof(Users.Email))]
    [IsRequired, IsStringLength(100), IsEmail]
    public string Email { get; init; } = "";

    /// <summary>
    /// Gets the login token sent to the user.
    /// </summary>
    [Display(ResourceType = typeof(Account), Name = nameof(Account.LoginCode))]
    [IsRequired, IsStringLength(100)]
    public string Token { get; init; } = "";

    /// <summary>
    /// Gets the URL to redirect to after successful login.
    /// </summary>
    public string? ReturnUrl { get; init; }
}
