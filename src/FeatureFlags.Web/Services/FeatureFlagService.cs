using System.Data;
using FeatureFlags.Domain;
using FeatureFlags.Domain.Models;
using FeatureFlags.Extensions.Services;
using FeatureFlags.Models;
using FeatureFlags.Resources;
using Microsoft.EntityFrameworkCore;

namespace FeatureFlags.Services;

/// <inheritdoc />
public sealed class FeatureFlagService(FeatureFlagsDbContext dbContext) : IFeatureFlagService {
    private readonly FeatureFlagsDbContext _DbContext = dbContext;

    /// <inheritdoc />
    public async Task<IEnumerable<FeatureFlagModel>> GetAllFeatureFlagsAsync(CancellationToken cancellationToken = default)
        => await _DbContext.FeatureFlags.SelectAsModel().ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<FeatureFlagModel?> GetFeatureFlagByNameAsync(string name, CancellationToken cancellationToken = default)
        => await _DbContext.FeatureFlags.SelectAsModel().FirstOrDefaultAsync(x => x.Name.ToLower() == name.ToLower(), cancellationToken);

    /// <inheritdoc />
    public async Task<(bool success, string message)> SaveFeatureFlagAsync(FeatureFlagModel featureFlagModel, CancellationToken cancellationToken = default) {
        if (featureFlagModel.Id > 0) {
            var featureFlag = await _DbContext.FeatureFlags.Where(x => x.Id == featureFlagModel.Id).FirstOrDefaultAsync(cancellationToken);
            if (featureFlag == null) {
                return (false, Core.ErrorInvalidId);
            }

            // prevent concurrent changes
            if ((featureFlag.UpdatedDate - featureFlagModel.UpdatedDate).Seconds > 0) {
                return (false, Core.ErrorConcurrency);
            }

            // check that name is unique
            if (!await IsUniqueNameAsync(featureFlagModel, cancellationToken)) {
                return (false, Flags.ErrorDuplicateName);
            }

            featureFlag.Name = featureFlagModel.Name;
            featureFlag.IsEnabled = featureFlagModel.IsEnabled;

            _DbContext.FeatureFlags.Update(featureFlag);
        } else {
            // check that name is unique
            if (!await IsUniqueNameAsync(featureFlagModel, cancellationToken)) {
                return (false, Flags.ErrorDuplicateName);
            }

            var featureFlag = new FeatureFlag {
                Name = featureFlagModel.Name,
                IsEnabled = featureFlagModel.IsEnabled
            };
            _DbContext.FeatureFlags.Add(featureFlag);
        }

        if (await _DbContext.SaveChangesAsync(cancellationToken) > 0) {
            return (true, Flags.SuccessSavingFlag);
        }
        return (false, Core.ErrorGeneric);
    }

    /// <summary>
    /// Validate that feature flag has a unique name.
    /// </summary>
    private async Task<bool> IsUniqueNameAsync(FeatureFlagModel featureFlagModel, CancellationToken cancellationToken = default) {
        var namedFlag = await _DbContext.FeatureFlags.FirstOrDefaultAsync(x => x.Name.ToLower() == featureFlagModel.Name.ToLower(), cancellationToken);
        return namedFlag == null || featureFlagModel.Id == namedFlag.Id;
    }

    /// <inheritdoc />
    public async Task<FeatureFlagModel?> GetFeatureFlagByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _DbContext.FeatureFlags.Where(x => x.Id == id).SelectAsModel().FirstOrDefaultAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<bool> DeleteFeatureFlagAsync(int id, CancellationToken cancellationToken = default) {
        // load feature flag so auditLog tracks it being deleted
        var feature = await _DbContext.FeatureFlags.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (feature == null) {
            return false;
        }

        _DbContext.FeatureFlags.Remove(feature);

        return await _DbContext.SaveChangesAsync(cancellationToken) > 0;
    }
}
