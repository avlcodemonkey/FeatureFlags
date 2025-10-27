using FeatureFlags.Validators;

namespace FeatureFlags.Models;

/// <summary>
/// Represents an api request.
/// </summary>
public sealed record ApiRequestModel {
    /// <summary>
    /// Gets the unique identifier for the request.
    /// </summary>
    [IsRequired]
    public int Id { get; init; }

    /// <summary>
    /// Gets the unique identifier for api key used for the request.
    /// </summary>
    [IsRequired]
    public int ApiKeyId { get; init; }

    /// <summary>
    /// Gets the IP associated with the API request.
    /// </summary>
    [IsRequired, IsStringLength(50)]
    public string IpAddress { get; init; } = "";

    /// <summary>
    /// Gets the date and time when the request was made.
    /// </summary>
    public DateTime RequestedDate { get; init; }
}
