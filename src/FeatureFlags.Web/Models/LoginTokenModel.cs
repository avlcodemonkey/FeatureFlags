using System.ComponentModel.DataAnnotations;
using FeatureFlags.Resources;
using FeatureFlags.Validators;

namespace FeatureFlags.Models;

public sealed record LoginTokenModel {
    [Display(ResourceType = typeof(Users), Name = nameof(Users.Email))]
    [IsRequired, IsStringLength(100), IsEmail]
    public string Email { get; init; } = "";

    [Display(ResourceType = typeof(Account), Name = nameof(Account.LoginCode))]
    [IsRequired, IsStringLength(100)]
    public string Token { get; init; } = "";

    public string? ReturnUrl { get; init; }
}
