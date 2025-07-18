using FeatureFlags.Validators;

namespace FeatureFlags.Models;

public sealed record RolePermissionModel : IAuditedModel, IEquatable<RolePermissionModel> {
    [IsRequired]
    public int Id { get; init; }

    [IsRequired]
    public int PermissionId { get; init; }

    [IsRequired]
    public int RoleId { get; init; }
}
