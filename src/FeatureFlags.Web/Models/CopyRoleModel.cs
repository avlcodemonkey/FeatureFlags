using System.ComponentModel.DataAnnotations;
using FeatureFlags.Resources;
using FeatureFlags.Validators;

namespace FeatureFlags.Models;

public sealed record CopyRoleModel : IAuditedModel {
    [IsRequired]
    public int Id { get; init; }

    [Display(ResourceType = typeof(Roles), Name = nameof(Roles.NewName))]
    [IsRequired, IsStringLength(100)]
    public string Prompt { get; init; } = "";
}
