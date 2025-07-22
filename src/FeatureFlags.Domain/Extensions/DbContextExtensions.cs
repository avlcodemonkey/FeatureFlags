using System.Reflection;
using System.Text.Json;
using FeatureFlags.Domain.Attributes;
using FeatureFlags.Domain.Models;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace FeatureFlags.Domain.Extensions;

public static class DbContextExtensions {
    private static readonly List<string> _UnauditedProperties = typeof(IAuditedEntity).GetProperties().Select(x => x.Name).ToList();

    public static int? GetPrimaryKey(this EntityEntry entry) {
        if (int.TryParse(entry.Properties.FirstOrDefault(x => x.Metadata.IsPrimaryKey())?.CurrentValue?.ToString() ?? "", out var primaryKey)) {
            return primaryKey;
        }
        return null;
    }

    public static string ToAuditJson(this IEnumerable<PropertyEntry> properties, bool currentValues = true)
        => JsonSerializer.Serialize(properties.Where(x => !_UnauditedProperties.Contains(x.Metadata.Name) && CanBeAudited(x))
            .ToDictionary(x => x.Metadata.Name, x => (currentValues ? x.CurrentValue : x.OriginalValue)?.ToString()));

    private static bool CanBeAudited(PropertyEntry propertyEntry) {
        var propertyInfo = propertyEntry.Metadata.PropertyInfo;
        return propertyInfo?.GetCustomAttribute<NoAuditAttribute>() == null;
    }
}
