using System.Data;
using System.Diagnostics.CodeAnalysis;
using FeatureFlags.Domain;
using FeatureFlags.Domain.Models;
using FeatureFlags.Extensions.Services;
using FeatureFlags.Models;
using FeatureFlags.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.FeatureManagement.FeatureFilters;

namespace FeatureFlags.Services;

/// <inheritdoc />
[SuppressMessage("Performance", "CA1862:Use the 'StringComparison' method overloads to perform case-insensitive string comparisons",
    Justification = "Linq can't translate stringComparison methods to sql.")
]
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

            await MapToEntity(featureFlagModel, featureFlag, cancellationToken);

            _DbContext.FeatureFlags.Update(featureFlag);
        } else {
            // check that name is unique
            if (!await IsUniqueNameAsync(featureFlagModel, cancellationToken)) {
                return (false, Flags.ErrorDuplicateName);
            }

            var featureFlag = new FeatureFlag();
            await MapToEntity(featureFlagModel, featureFlag, cancellationToken);
            _DbContext.FeatureFlags.Add(featureFlag);
        }

        if (await _DbContext.SaveChangesAsync(cancellationToken) > 0) {
            return (true, Flags.SuccessSavingFlag);
        }
        return (false, Core.ErrorGeneric);
    }

    /// <summary>
    /// Maps data from the feature flag model onto the feature flag entity.
    /// </summary>
    private async Task MapToEntity(FeatureFlagModel featureFlagModel, FeatureFlag featureFlag, CancellationToken cancellationToken = default) {
        featureFlag.Name = featureFlagModel.Name;
        featureFlag.Status = featureFlagModel.Status;
        featureFlag.RequirementType = (int)featureFlagModel.RequirementType;

        // Map filters
        var existingFilters = new Dictionary<int, FeatureFlagFilter>();
        if (featureFlag.Id > 0 && featureFlagModel.Filters?.Any() == true) {
            existingFilters = (await _DbContext.FeatureFlagFilters.Include(x => x.Users).Where(x => x.FeatureFlagId == featureFlag.Id).ToListAsync(cancellationToken))
                .ToDictionary(x => x.Id, x => x);
        }

        featureFlag.Filters = featureFlagModel.Filters?.Select(filterModel => {
            FeatureFlagFilter filter;
            if (filterModel.Id > 0 && existingFilters.TryGetValue(filterModel.Id, out var existingFilter)) {
                filter = existingFilter;
            } else {
                filter = new FeatureFlagFilter { FeatureFlagId = featureFlag.Id };
            }

            filter.FilterType = (int)filterModel.FilterType;
            MapTargetingFilter(filterModel, filter);
            MapTimeWindowFilter(filterModel, filter);
            MapPercentageFilter(filterModel, filter);
            MapJSONFilter(filterModel, filter);

            return filter;
        }).ToList() ?? [];
    }

    /// <summary>
    /// Maps the targeting filter configuration from the specified <see cref="FeatureFlagFilterModel"/> to the provided
    /// <see cref="FeatureFlagFilter"/> instance.
    /// </summary>
    /// <remarks>This method updates the <paramref name="filter"/> to reflect the targeting rules defined in <paramref name="filterModel"/>.
    /// If the filter type is targeting, users specified in the <c>TargetUsers</c> and <c>ExcludeUsers</c> collections of <paramref name="filterModel"/>
    /// are added to the <c>Users</c> collection of <paramref name="filter"/> with their inclusion status set accordingly. If the filter type is not
    /// targeting, the <c>Users</c> collection is cleared.</remarks>
    /// <param name="filterModel">The source model containing the targeting filter configuration, including users to include or exclude.</param>
    /// <param name="filter">The destination filter object to which the targeting configuration will be applied.</param>
    private static void MapTargetingFilter(FeatureFlagFilterModel filterModel, FeatureFlagFilter filter) {
        if (filterModel.FilterType == Constants.FilterTypes.Targeting) {
            var users = new List<FeatureFlagFilterUser>();
            users.AddRange(filterModel.TargetUsers?.Where(u => !string.IsNullOrWhiteSpace(u)).Select(u => {
                var user = filter.Users.FirstOrDefault(x => x.User == u) ?? new FeatureFlagFilterUser { FeatureFlagFilterId = filterModel.Id, User = u };
                user.Include = true;
                return user;
            }) ?? []);
            users.AddRange(filterModel.ExcludeUsers?.Where(u => !string.IsNullOrWhiteSpace(u)).Select(u => {
                var user = filter.Users.FirstOrDefault(x => x.User == u) ?? new FeatureFlagFilterUser { FeatureFlagFilterId = filterModel.Id, User = u };
                user.Include = false;
                return user;
            }) ?? []);
            filter.Users = users;
        } else {
            filter.Users = [];
        }
    }

    /// <summary>
    /// Maps the properties of a time window filter from a <see cref="FeatureFlagFilterModel"/> to a <see cref="FeatureFlagFilter"/>.
    /// </summary>
    /// <remarks>This method maps time window-related properties only if the <paramref name="filterModel"/>
    /// has a filter type of <see cref="Constants.FilterTypes.TimeWindow"/>. If the filter type does not match, all time
    /// window-related properties in the <paramref name="filter"/> are set to <see langword="null"/>.</remarks>
    /// <param name="filterModel">The source filter model containing the time window configuration to map.</param>
    /// <param name="filter">The target filter object to populate with the mapped time window properties.</param>
    private static void MapTimeWindowFilter(FeatureFlagFilterModel filterModel, FeatureFlagFilter filter) {
        if (filterModel.FilterType != Constants.FilterTypes.TimeWindow) {
            ClearTimeWindowProperties(filter);
            return;
        }

        filter.TimeStart = filterModel.TimeStart;
        filter.TimeEnd = filterModel.TimeEnd;
        filter.TimeRecurrenceType = filterModel.TimeRecurrenceType.HasValue ? (int)filterModel.TimeRecurrenceType.Value : null;

        if (!filterModel.TimeRecurrenceType.HasValue) {
            ClearRecurrenceProperties(filter);
            return;
        }

        filter.TimeRecurrenceInterval = filterModel.TimeRecurrenceInterval;
        if (filterModel.TimeRecurrenceType == RecurrencePatternType.Weekly) {
            filter.TimeRecurrenceDaysOfWeek = filterModel.TimeRecurrenceDaysOfWeek != null
                ? string.Join(",", filterModel.TimeRecurrenceDaysOfWeek)
                : null;
            filter.TimeRecurrenceFirstDayOfWeek = filterModel.TimeRecurrenceFirstDayOfWeek;
        } else {
            filter.TimeRecurrenceDaysOfWeek = null;
            filter.TimeRecurrenceFirstDayOfWeek = null;
        }

        filter.TimeRecurrenceRangeType = filterModel.TimeRecurrenceRangeType.HasValue ? (int)filterModel.TimeRecurrenceRangeType.Value : null;
        if (!filterModel.TimeRecurrenceRangeType.HasValue) {
            filter.TimeRecurrenceEndDate = null;
            filter.TimeRecurrenceNumberOfOccurrences = null;
            return;
        }

        filter.TimeRecurrenceEndDate = filterModel.TimeRecurrenceRangeType == RecurrenceRangeType.EndDate
            ? filterModel.TimeRecurrenceEndDate
            : null;
        filter.TimeRecurrenceNumberOfOccurrences = filterModel.TimeRecurrenceRangeType == RecurrenceRangeType.Numbered
            ? filterModel.TimeRecurrenceNumberOfOccurrences
            : null;
    }

    /// <summary>
    /// Clears all time window-related properties from the specified feature flag filter.
    /// </summary>
    /// <param name="filter">The feature flag filter whose time window properties will be cleared. Cannot be null.</param>
    private static void ClearTimeWindowProperties(FeatureFlagFilter filter) {
        filter.TimeStart = null;
        filter.TimeEnd = null;
        filter.TimeRecurrenceType = null;
        ClearRecurrenceProperties(filter);
    }

    /// <summary>
    /// Clears all recurrence-related properties of the specified <see cref="FeatureFlagFilter"/>.
    /// </summary>
    /// <param name="filter">The <see cref="FeatureFlagFilter"/> whose recurrence properties will be cleared. Cannot be <c>null</c>.</param>
    private static void ClearRecurrenceProperties(FeatureFlagFilter filter) {
        filter.TimeRecurrenceInterval = null;
        filter.TimeRecurrenceDaysOfWeek = null;
        filter.TimeRecurrenceFirstDayOfWeek = null;
        filter.TimeRecurrenceRangeType = null;
        filter.TimeRecurrenceEndDate = null;
        filter.TimeRecurrenceNumberOfOccurrences = null;
    }

    /// <summary>
    /// Maps the percentage filter value from the specified <see cref="FeatureFlagFilterModel"/> to the provided <see cref="FeatureFlagFilter"/>.
    /// </summary>
    /// <remarks>If the <paramref name="filterModel"/> has a filter type of <c>Percentage</c>, the <c>PercentageValue</c> of the
    /// <paramref name="filter"/> is set to the value from <paramref name="filterModel"/>. Otherwise, the <c>PercentageValue</c> of the
    /// <paramref name="filter"/> is set to <see langword="null"/>.</remarks>
    /// <param name="filterModel">The source model containing the filter type and percentage value to be mapped.</param>
    /// <param name="filter">The target filter object where the percentage value will be set.</param>
    private static void MapPercentageFilter(FeatureFlagFilterModel filterModel, FeatureFlagFilter filter) {
        if (filterModel.FilterType == Constants.FilterTypes.Percentage) {
            filter.PercentageValue = filterModel.PercentageValue;
        } else {
            filter.PercentageValue = null;
        }
    }

    /// <summary>
    /// Maps the JSON filter data from a <see cref="FeatureFlagFilterModel"/> to a <see cref="FeatureFlagFilter"/>.
    /// </summary>
    /// <remarks>If the <paramref name="filterModel"/> has a filter type of <see cref="Constants.FilterTypes.JSON"/>,
    /// the JSON data is copied to the <paramref name="filter"/>. Otherwise, the JSON property of the <paramref name="filter"/>
    /// is set to <see langword="null"/>.</remarks>
    /// <param name="filterModel">The source model containing the filter data to map.</param>
    /// <param name="filter">The target filter object to which the JSON data will be applied.</param>
    private static void MapJSONFilter(FeatureFlagFilterModel filterModel, FeatureFlagFilter filter) {
        if (filterModel.FilterType == Constants.FilterTypes.JSON) {
            // JSON
            filter.JSON = filterModel.JSON;
        } else {
            filter.JSON = null;
        }
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
