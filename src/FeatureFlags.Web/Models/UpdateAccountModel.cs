using System.ComponentModel.DataAnnotations;
using FeatureFlags.Resources;
using FeatureFlags.Validators;

namespace FeatureFlags.Models;

/// <summary>
/// Represents a model for updating account information.
/// </summary>
public sealed record UpdateAccountModel : IAuditedModel {
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateAccountModel"/> class.
    /// </summary>
    public UpdateAccountModel() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateAccountModel"/> class using the specified user information.
    /// </summary>
    /// <param name="user">User information used to populate the account update model.  The <see cref="UserModel.Email"/>, <see
    /// cref="UserModel.Name"/>, and <see cref="UserModel.LanguageId"/> properties are copied to the corresponding
    /// properties of this instance.</param>
    public UpdateAccountModel(UserModel user) {
        Email = user.Email;
        Name = user.Name;
        LanguageId = user.LanguageId;
    }

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
}
