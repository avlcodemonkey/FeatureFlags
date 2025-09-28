using FeatureFlags.Client;
using FeatureFlags.Services;
using Microsoft.FeatureManagement;

namespace FeatureFlags.Utils;

/// <summary>
/// Provides feature definitions by interacting with the internal feature flag service.
/// </summary>
/// <param name="featureFlagService">The feature flag service used to retrieve feature definitions from the db.</param>
public class InternalFeatureDefinitionProvider(IFeatureFlagService featureFlagService) : IFeatureDefinitionProvider {
    private readonly IFeatureFlagService _FeatureFlagService = featureFlagService;

    /// <summary>
    /// Retrieves all feature definitions asynchronously as an asynchronous stream.
    /// </summary>
    /// <remarks>This method returns an <see cref="IAsyncEnumerable{T}"/> of <see cref="FeatureDefinition"/>, 
    /// allowing the caller to enumerate feature definitions as they are retrieved.  The enumeration is performed
    /// lazily, and the caller can process each feature definition  without waiting for the entire collection to be
    /// loaded.</remarks>
    /// <returns>An <see cref="IAsyncEnumerable{T}"/> of <see cref="FeatureDefinition"/> representing all feature definitions.</returns>
    public async IAsyncEnumerable<FeatureDefinition> GetAllFeatureDefinitionsAsync() {
        var featureFlags = await _FeatureFlagService.GetAllFeatureFlagsAsync();
        foreach (var featureFlag in featureFlags) {
            yield return FeatureDefinitionMapper.ToFeatureDefinition(CustomFeatureDefinitionMapper.MapToCustomFeatureDefinition(featureFlag));
        }
    }

    /// <summary>
    /// Retrieves the definition of a feature by its name asynchronously.
    /// </summary>
    /// <remarks>This method queries the feature flag client to retrieve the definition of the specified
    /// feature. Ensure that the feature name provided is valid and corresponds to an existing feature.</remarks>
    /// <param name="featureName">The name of the feature to retrieve. Cannot be null or empty.</param>
    /// <returns>A <see cref="FeatureDefinition"/> object representing the feature's definition if found, else an empty one is created</returns>
    public async Task<FeatureDefinition> GetFeatureDefinitionAsync(string featureName) {
        var flag = await _FeatureFlagService.GetFeatureFlagByNameAsync(featureName);
        if (flag == null) {
            return new FeatureDefinition { Name = featureName };
        }
        return FeatureDefinitionMapper.ToFeatureDefinition(CustomFeatureDefinitionMapper.MapToCustomFeatureDefinition(flag));
    }
}
