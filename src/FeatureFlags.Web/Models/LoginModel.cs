using System.ComponentModel.DataAnnotations;
using FeatureFlags.Resources;
using FeatureFlags.Validators;

namespace FeatureFlags.Models;

/// <summary>
/// Model representing user login data.
/// </summary>
public sealed record LoginModel {
    /// <summary>
    /// Gets the email address of the user.
    /// </summary>
    [Display(ResourceType = typeof(Users), Name = nameof(Users.Email))]
    [IsRequired, IsStringLength(100), IsEmail]
    public string Email { get; init; } = "";

    /// <summary>
    /// Gets the URL to redirect to after successful login.
    /// </summary>
    public string? ReturnUrl { get; init; }
}
