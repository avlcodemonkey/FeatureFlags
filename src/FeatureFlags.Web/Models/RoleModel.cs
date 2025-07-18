using System.ComponentModel.DataAnnotations;
using FeatureFlags.Resources;
using FeatureFlags.Validators;

namespace FeatureFlags.Models;

public sealed record RoleModel : IAuditedModel, IVersionedModel {
    [IsRequired]
    public int Id { get; init; }

    [Display(ResourceType = typeof(Roles), Name = nameof(Roles.Name))]
    [IsRequired, IsStringLength(100)]
    public string Name { get; init; } = "";

    [Display(ResourceType = typeof(Roles), Name = nameof(Roles.IsDefault))]
    public bool IsDefault { get; init; } = false;

    [Display(ResourceType = typeof(Roles), Name = nameof(Roles.Permissions))]
    public IEnumerable<int>? PermissionIds { get; init; }

    public DateTime UpdatedDate { get; init; }
}
