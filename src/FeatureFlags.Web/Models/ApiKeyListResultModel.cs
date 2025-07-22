namespace FeatureFlags.Models;

/// <summary>
/// Subset of ApiKey used only for showing the api key list.
/// </summary>
public sealed record ApiKeyListResultModel : IAuditedModel {
    /// <summary>
    /// Gets the unique identifier for the api kye.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Gets the name associated with the api key.
    /// </summary>
    public string Name { get; init; } = "";

    /// <summary>
    /// Gets the unique key associated with this instance.
    /// </summary>
    public string Key { get; init; } = "";

    /// <summary>
    /// Gets the date and time when the api key was created.
    /// </summary>
    public DateTime CreatedDate { get; init; }

}
