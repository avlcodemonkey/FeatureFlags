namespace FeatureFlags.Domain.Attributes;

/// <summary>
/// Configures the default value expression for the column that the property maps to when targeting a relational database.
/// </summary>
/// <remarks>
/// Attribute equivalent of https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.relationalpropertybuilderextensions.hasdefaultvaluesql?view=efcore-7.0
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class DefaultValueSqlAttribute : Attribute {
    public string Sql { get; }

    public DefaultValueSqlAttribute(string sql) {
        if (string.IsNullOrWhiteSpace(sql)) {
            throw new ArgumentException("SQL cannot be empty.", nameof(sql));
        }

        Sql = sql;
    }
}
