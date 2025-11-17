namespace FeatureFlags.Models;

/// <summary>
/// Model containing chart data to display to the user.
/// </summary>
public sealed record DashboardModel {
    /// <summary>
    /// Gets the data for the requests by API key chart.
    /// </summary>
    public required ChartModel RequestsByApiKey { get; init; }

    /// <summary>
    /// Gets the data for the requests by IP address chart.
    /// </summary>
    public required ChartModel RequestsByIpAddress { get; init; }
}
