namespace FeatureFlags.Models;

/// <summary>
/// Subset of ApiKey used only for showing the API key list.
/// </summary>
public sealed record ApiKeyListResultModel : IAuditedModel {
    /// <summary>
    /// Gets the unique identifier for the API key.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Gets the name associated with the API key.
    /// </summary>
    public string Name { get; init; } = "";

    /// <summary>
    /// Gets the unique key associated with this API key.
    /// </summary>
    public string Key { get; init; } = "";

    /// <summary>
    /// Gets the date and time when the API key was created.
    /// </summary>
    public DateTime CreatedDate { get; init; }
}
