using System.Globalization;
using FeatureFlags.Models;
using FeatureFlags.Services;

namespace FeatureFlags.Utils;

public sealed class FeatureFlagManager(IFeatureFlagService featureFlagService) {
    private readonly IFeatureFlagService _FeatureFlagService = featureFlagService;

    /// <summary>
    /// Reconciles the database with the list of feature flags provided.
    /// </summary>
    public async Task<bool> RegisterAsync(Dictionary<string, string> featureFlags, CancellationToken cancellationToken = default) {
        // query all flags from db
        var flags = (await _FeatureFlagService.GetAllFeatureFlagsAsync(cancellationToken))
            .ToDictionary(x => $"{x.Name?.Trim()}".ToLower(CultureInfo.InvariantCulture), x => x);

        // save any flags not in db
        var missingFlagList = featureFlags.Where(x => !flags.ContainsKey(x.Key));
        foreach (var flag in missingFlagList) {
            var (success, _) = await _FeatureFlagService.SaveFeatureFlagAsync(new FeatureFlagModel { Name = flag.Value, IsEnabled = false }, cancellationToken);
            if (!success) {
                return false;
            }
        }

        // delete any flag not in flag list
        var removedFlags = flags.Where(x => !featureFlags.ContainsKey(x.Key));
        foreach (var flag in removedFlags) {
            if (!await _FeatureFlagService.DeleteFeatureFlagAsync(flag.Value.Id, cancellationToken)) {
                return false;
            }
        }

        return true;
    }
}
