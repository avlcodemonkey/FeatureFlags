using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;

namespace FeatureFlags.Client;

/// <inheritdoc />
public class HttpFeatureFlagClient(IHttpClientFactory httpClientFactory, ILogger<HttpFeatureFlagClient> logger) : IFeatureFlagClient {
    private readonly IHttpClientFactory _HttpClientFactory = httpClientFactory;
    private readonly ILogger<HttpFeatureFlagClient> _Logger = logger;

    /// <inheritdoc />
    public async Task<List<FeatureDefinition>> GetAllFeatureDefinitionsAsync(CancellationToken cancellationToken = default) {
        var httpClient = _HttpClientFactory.CreateClient(Constants.HttpClientName);

        // fetch the feature flag definitions from the API
        try {
            using var response = await httpClient.GetAsync("features", cancellationToken);
            response.EnsureSuccessStatusCode();

            var featureFlags = await response.Content.ReadFromJsonAsync<List<CustomFeatureDefinition>>(cancellationToken) ?? [];
            var featureDefinitions = featureFlags.Select(x => x.ToFeatureDefinition()).ToList();

            // @todo add caching

            return featureDefinitions;
        } catch (Exception ex) {
            _Logger.LogError(ex, "Error fetching feature definitions");
            return [];
        }
    }

    /// <inheritdoc />
    public async Task<FeatureDefinition?> GetFeatureDefinitionByNameAsync(string name, CancellationToken cancellationToken = default) {
        var httpClient = _HttpClientFactory.CreateClient(Constants.HttpClientName);

        // fetch the feature flag definition from the API
        try {
            using var response = await httpClient.GetAsync($"feature/{name}", cancellationToken);
            response.EnsureSuccessStatusCode();

            var featureFlag = await response.Content.ReadFromJsonAsync<CustomFeatureDefinition>(cancellationToken);
            var featureDefinition = featureFlag?.ToFeatureDefinition();

            // @todo add caching

            return featureDefinition;
        } catch (Exception ex) {
            _Logger.LogError(ex, "Error fetching feature definition for '{Name}'", name);
            return null;
        }
    }

    /// <inheritdoc />
    public Task<bool> ClearCache() => throw new NotImplementedException();
}
