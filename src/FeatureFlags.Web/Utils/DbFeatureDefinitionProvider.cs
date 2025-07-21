using FeatureFlags.Services;
using Microsoft.FeatureManagement;

namespace FeatureFlags.Utils;

public class DbFeatureDefinitionProvider(IFeatureFlagService featureFlagService) : IFeatureDefinitionProvider {
    private readonly IFeatureFlagService _featureFlagService = featureFlagService;

    public async IAsyncEnumerable<FeatureDefinition> GetAllFeatureDefinitionsAsync() {
        var featureFlags = await _featureFlagService.GetAllFeatureFlagsAsync();
        var definitions = featureFlags.Select(x => new FeatureDefinition {
            Name = x.Name,
            EnabledFor = x.IsEnabled ? new[] { new FeatureFilterConfiguration { Name = "AlwaysOn" } } : null
        });

        foreach (var featureDefinition in definitions) {
            yield return featureDefinition;
        }
    }

    public async Task<FeatureDefinition> GetFeatureDefinitionAsync(string featureName) {
        var featureFlags = await _featureFlagService.GetAllFeatureFlagsAsync();
        var featureFlag = featureFlags.FirstOrDefault(x => x.Name == featureName);
        if (featureFlag == null) {
            return new FeatureDefinition { Name = featureName };
        }

        return new FeatureDefinition {
            Name = featureFlag.Name,
            EnabledFor = featureFlag.IsEnabled ? new[] { new FeatureFilterConfiguration { Name = "AlwaysOn" } } : null
        };
    }
}
