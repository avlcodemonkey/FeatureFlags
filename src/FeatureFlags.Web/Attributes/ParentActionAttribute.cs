namespace FeatureFlags.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public sealed class ParentActionAttribute(string action) : Attribute {
    public string Action { get; set; } = action;
}
