using System.ComponentModel.DataAnnotations;
using FeatureFlags.Resources;
using FeatureFlags.Validators;

namespace FeatureFlags.Models;

public sealed record RegisterModel {
    [Display(ResourceType = typeof(Users), Name = nameof(Users.Email))]
    [IsRequired, IsStringLength(100), IsEmail]
    public string Email { get; init; } = "";

    [Display(ResourceType = typeof(Users), Name = nameof(Users.Name))]
    [IsRequired, IsStringLength(100)]
    public string Name { get; init; } = "";

    [Display(ResourceType = typeof(Users), Name = nameof(Users.Language))]
    [IsRequired]
    public int LanguageId { get; init; }
}
