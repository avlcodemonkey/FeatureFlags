using FeatureFlags.Constants;

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
    public string ChartUrl { get; init; } = "";

    /// <summary>
    /// Gets the chart type to be used for rendering the data.
    /// </summary>
    public ChartTypes ChartType { get; init; } = ChartTypes.Bar;

    /// <summary>
    /// Gets or sets a value indicating whether the heading is displayed.
    /// </summary>
    public bool ShowHeading { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether labels are displayed.
    /// </summary>
    public bool ShowLabels { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the primary axis is displayed.
    /// </summary>
    public bool ShowPrimaryAxis { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether 4 secondary axes are displayed.
    /// </summary>
    public bool ShowSecondaryAxes { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether data axes are displayed.
    /// </summary>
    public bool ShowDataAxes { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether data spacing is applied.
    /// </summary>
    public bool DataSpacing { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether data is hidden.
    /// </summary>
    public bool HideData { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether data is displayed on hover.
    /// </summary>
    public bool ShowDataOnHover { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the legend should be displayed.
    /// </summary>
    public bool ShowLegend { get; set; }
}
