using System.ComponentModel.DataAnnotations;
using FeatureFlags.Resources;
using FeatureFlags.Validators;

namespace FeatureFlags.Models;

/// <summary>
/// Model representing user registration data.
/// </summary>
public sealed record RegisterModel {
    /// <summary>
    /// Gets the email address of the user.
    /// </summary>
    [Display(ResourceType = typeof(Users), Name = nameof(Users.Email))]
    [IsRequired, IsStringLength(100), IsEmail]
    public string Email { get; init; } = "";

    /// <summary>
    /// Gets the display name of the user.
    /// </summary>
    [Display(ResourceType = typeof(Users), Name = nameof(Users.Name))]
    [IsRequired, IsStringLength(100)]
    public string Name { get; init; } = "";

    /// <summary>
    /// Gets the identifier of the user's preferred language.
    /// </summary>
    [Display(ResourceType = typeof(Users), Name = nameof(Users.Language))]
    [IsRequired]
    public int LanguageId { get; init; }
}
