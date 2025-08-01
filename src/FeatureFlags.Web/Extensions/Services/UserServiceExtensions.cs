using FeatureFlags.Domain.Models;
using FeatureFlags.Models;
using FeatureFlags.Utils;

namespace FeatureFlags.Extensions.Services;

/// <summary>
/// Provides extension methods for querying and transforming <see cref="User"/> entities into specific models.
/// </summary>
public static class UserServiceExtensions {
    /// <summary>
    /// Projects a queryable sequence of <see cref="User"/> entities into a sequence of <see cref="UserModel"/> objects.
    /// </summary>
    /// <param name="query"><see cref="IQueryable{T}"/> sequence of <see cref="User"/> entities to project.</param>
    /// <returns><see cref="IQueryable{T}"/> sequence of <see cref="UserModel"/> objects.</returns>
    public static IQueryable<UserModel> SelectAsModel(this IQueryable<User> query)
        => query.Select(x => new UserModel {
            Id = x.Id, Email = x.Email, Name = x.Name,
            LanguageId = x.LanguageId, RoleIds = x.UserRoles.Select(x => x.RoleId), UpdatedDate = x.UpdatedDate
        });

    /// <summary>
    /// Projects a queryable collection of <see cref="User"/> entities into a collection of <see cref="AutocompleteUserModel"/> objects.
    /// </summary>
    /// <param name="query">Queryable collection of <see cref="User"/> entities to project.</param>
    /// <returns><see cref="IQueryable{T}"/> of <see cref="AutocompleteUserModel"/> objects.</returns>
    public static IQueryable<AutocompleteUserModel> SelectAsAuditLogUserModel(this IQueryable<User> query)
        => query.Select(x => new AutocompleteUserModel { Value = x.Id, Label = NameHelper.DisplayName(x.Name, x.Email) });
}
