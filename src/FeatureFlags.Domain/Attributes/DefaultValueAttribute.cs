namespace FeatureFlags.Domain.Attributes;

/// <summary>
/// Configures the default value for the column that the property maps to when targeting a relational database.
/// </summary>
/// <remarks>
/// Attribute equivalent of https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.relationalpropertybuilderextensions.hasdefaultvalue?view=efcore-7.0
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class DefaultValueAttribute(object defaultValue) : Attribute {
    public object DefaultValue { get; } = defaultValue ?? throw new ArgumentException("Default value cannot be empty.", nameof(defaultValue));
}
