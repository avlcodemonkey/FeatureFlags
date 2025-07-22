namespace FeatureFlags.Domain.Attributes;

/// <summary>
/// Specifies that the property or field should not be included in audit logs.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class NoAuditAttribute() : Attribute {
}
