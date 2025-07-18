using FeatureFlags.Domain.Models;
using FeatureFlags.Models;
using FeatureFlags.Utils;

namespace FeatureFlags.Extensions.Services;

public static class UserServiceExtensions {
    public static IQueryable<UserModel> SelectAsModel(this IQueryable<User> query)
        => query.Select(x => new UserModel {
            Id = x.Id, Email = x.Email, Name = x.Name,
            LanguageId = x.LanguageId, RoleIds = x.UserRoles.Select(x => x.RoleId), UpdatedDate = x.UpdatedDate
        });

    // @todo see if there are other userService methods that should use this instead
    public static IQueryable<AutocompleteUserModel> SelectAsAuditLogUserModel(this IQueryable<User> query)
        => query.Select(x => new AutocompleteUserModel { Value = x.Id, Label = NameHelper.DisplayName(x.Name, x.Email) });
}
