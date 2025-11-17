namespace FeatureFlags.Models;

/// <summary>
/// Model representing a single chart to display to the user.
/// </summary>
public sealed record ChartModel {
    /// <summary>
    /// Gets the title of the chart
    /// </summary>
    public string Title { get; init; } = "";

    /// <summary>
    /// Gets the URL to fetch data for the chart.
    /// </summary>
    public string SourceUrl { get; init; } = "";
}
