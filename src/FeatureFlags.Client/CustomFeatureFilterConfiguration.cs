namespace FeatureFlags.Client;

/// <summary>
/// Custom version of FeatureFilterConfiguration that can be deserialized from JSON.
/// </summary>
public class CustomFeatureFilterConfiguration {
    /// <summary>
    /// Gets or sets the name of the feature filter.
    /// </summary>
    public string Name { get; set; } = "";

    // @todo this causes a json deserialization error. determine if its needed.
    /*
    /// <summary>
    /// Gets or sets the configurable parameters that can vary across instances of a feature filter.
    /// </summary>
    public ConfigurationRoot Parameters { get; set; } = new ConfigurationRoot(new List<IConfigurationProvider>());
    */
}
