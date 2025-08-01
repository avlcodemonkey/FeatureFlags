using System.Diagnostics.CodeAnalysis;
using FeatureFlags.Domain;
using FeatureFlags.Domain.Models;
using FeatureFlags.Extensions.Services;
using FeatureFlags.Models;
using FeatureFlags.Resources;
using FeatureFlags.Utils;
using Microsoft.EntityFrameworkCore;

namespace FeatureFlags.Services;

/// <inheritdoc />
[SuppressMessage("Performance", "CA1862:Use the 'StringComparison' method overloads to perform case-insensitive string comparisons",
    Justification = "Linq can't translate stringComparison methods to sql.")
]
public sealed class ApiKeyService(FeatureFlagsDbContext dbContext) : IApiKeyService {
    private readonly FeatureFlagsDbContext _DbContext = dbContext;

    /// <inheritdoc />
    public async Task<IEnumerable<ApiKeyModel>> GetAllApiKeysAsync(CancellationToken cancellationToken = default)
        => await _DbContext.ApiKeys.SelectAsModel().ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<ApiKeyModel?> GetApiKeyByKeyAsync(string key, CancellationToken cancellationToken = default) {
        var hashedKey = KeyGenerator.GetSha512Hash(key);
        var apiKey = await _DbContext.ApiKeys.FirstOrDefaultAsync(x => x.Key == hashedKey, cancellationToken);
        if (apiKey == null) {
            return null;
        }
        return new ApiKeyModel {
            Id = apiKey.Id, Name = apiKey.Name, Key = new string('x', 32),
            CreatedDate = apiKey.CreatedDate, UpdatedDate = apiKey.UpdatedDate
        };
    }

    /// <inheritdoc />
    public async Task<(bool success, string message)> SaveApiKeyAsync(ApiKeyModel apiKeyModel, CancellationToken cancellationToken = default) {
        if (apiKeyModel.Id > 0) {
            throw new NotImplementedException("Updating API keys is not possible.");
        } else {
            // check that name is unique
            if (!await IsUniqueNameAsync(apiKeyModel, cancellationToken)) {
                return (false, ApiKeys.ErrorDuplicateName);
            }

            var hashedKey = KeyGenerator.GetSha512Hash(apiKeyModel.Key);
            var apiKey = new ApiKey {
                Name = apiKeyModel.Name,
                Key = hashedKey
            };
            _DbContext.ApiKeys.Add(apiKey);
        }

        if (await _DbContext.SaveChangesAsync(cancellationToken) > 0) {
            return (true, ApiKeys.SuccessSavingApiKey);
        }
        return (false, Core.ErrorGeneric);
    }

    /// <summary>
    /// Validate that API key has a unique name.
    /// </summary>
    private async Task<bool> IsUniqueNameAsync(ApiKeyModel apiKeyModel, CancellationToken cancellationToken = default) {
        var namedApiKey = await _DbContext.ApiKeys.FirstOrDefaultAsync(x => x.Name.ToLower() == apiKeyModel.Name.ToLower(), cancellationToken);
        return namedApiKey == null || apiKeyModel.Id == namedApiKey.Id;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteApiKeyAsync(int id, CancellationToken cancellationToken = default) {
        // load apiKey so auditLog tracks it being deleted
        var apiKey = await _DbContext.ApiKeys.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (apiKey == null) {
            return false;
        }

        _DbContext.ApiKeys.Remove(apiKey);

        return await _DbContext.SaveChangesAsync(cancellationToken) > 0;
    }
}
