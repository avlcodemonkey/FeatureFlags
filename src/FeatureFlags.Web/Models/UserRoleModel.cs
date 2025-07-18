using FeatureFlags.Validators;

namespace FeatureFlags.Models;

public sealed record UserRoleModel : IAuditedModel {
    [IsRequired]
    public int Id { get; init; }

    [IsRequired]
    public int RoleId { get; init; }

    [IsRequired]
    public int UserId { get; init; }
}
