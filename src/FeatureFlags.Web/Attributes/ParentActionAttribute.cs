namespace FeatureFlags.Attributes;

/// <summary>
/// Specifies the name of the parent action associated with a method.
/// </summary>
/// <param name="action">The name of the parent action. This value cannot be null or empty.</param>
[AttributeUsage(AttributeTargets.Method)]
public sealed class ParentActionAttribute(string action) : Attribute {
    /// <summary>
    /// Gets or sets the action to be performed.
    /// </summary>
    public string Action { get; set; } = action;
}
