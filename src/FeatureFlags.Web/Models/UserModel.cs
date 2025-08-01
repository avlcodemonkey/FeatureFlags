using System.ComponentModel.DataAnnotations;
using FeatureFlags.Resources;
using FeatureFlags.Validators;

namespace FeatureFlags.Models;

/// <summary>
/// Represents a user in the system with associated metadata, including audit and versioning information.
/// </summary>
public sealed record UserModel : IAuditedModel, IVersionedModel {
    /// <summary>
    /// Gets the unique identifier for the entity.
    /// </summary>
    [IsRequired]
    public int Id { get; init; }

    /// <summary>
    /// Gets the email address of the user.
    /// </summary>
    [Display(ResourceType = typeof(Users), Name = nameof(Users.Email))]
    [IsRequired, IsStringLength(100), IsEmail]
    public string Email { get; init; } = "";

    /// <summary>
    /// Gets the name of the user.
    /// </summary>
    [Display(ResourceType = typeof(Users), Name = nameof(Users.Name))]
    [IsRequired, IsStringLength(100)]
    public string Name { get; init; } = "";

    /// <summary>
    /// Gets the identifier of the language associated with the user.
    /// </summary>
    [Display(ResourceType = typeof(Users), Name = nameof(Users.Language))]
    [IsRequired]
    public int LanguageId { get; init; }

    /// <summary>
    /// Gets the collection of role IDs associated with the user.
    /// </summary>
    [Display(ResourceType = typeof(Users), Name = nameof(Users.Roles))]
    public IEnumerable<int>? RoleIds { get; init; }

    /// <summary>
    /// Gets the date and time when the entity was last updated.
    /// </summary>
    public DateTime UpdatedDate { get; init; }
}
