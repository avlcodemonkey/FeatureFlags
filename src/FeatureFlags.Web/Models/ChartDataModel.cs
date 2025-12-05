namespace FeatureFlags.Models;

/// <summary>
/// Model representing a single data point for a chart.
/// </summary>
public sealed record ChartDataModel {
    /// <summary>
    /// Starting position of the data point, used for rendering charts.
    /// </summary>
    public string Start { get; init; } = "";

    /// <summary>
    /// Size of the data point, used for rendering charts.
    /// </summary>
    public string Size { get; init; } = "";

    /// <summary>
    /// Tooltip text for the data point.
    /// </summary>
    public string Tooltip { get; init; } = "";

    /// <summary>
    /// Label associated with the data point.
    /// </summary>
    public string Label { get; init; } = "";

    /// <summary>
    /// Color associated with the data point, used for rendering charts.
    /// </summary>
    public string Color { get; init; } = "";
}
