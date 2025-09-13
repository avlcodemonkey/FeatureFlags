using FeatureFlags.Domain.Models;
using FeatureFlags.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.FeatureManagement.FeatureFilters;

namespace FeatureFlags.Extensions.Services;

/// <summary>
/// Provides extension methods for querying feature flags.
/// </summary>
public static class FeatureFlagServiceExtensions {
    /// <summary>
    /// Projects a queryable collection of <see cref="FeatureFlag"/> entities into a collection of <see cref="FeatureFlagModel"/> objects.
    /// </summary>
    /// <param name="query">Queryable collection of <see cref="FeatureFlag"/> entities to project.</param>
    /// <returns><see cref="IQueryable{T}"/> containing <see cref="FeatureFlagModel"/> objects.</returns>
    public static IQueryable<FeatureFlagModel> SelectAsModel(this IQueryable<FeatureFlag> query)
        => query
            .Include(x => x.Filters)
            .ThenInclude(f => f.Users)
            .Select(x => new FeatureFlagModel {
                Id = x.Id,
                Name = x.Name,
                Status = x.Status,
                RequirementType = (Constants.RequirementType)x.RequirementType,
                Filters = x.Filters.Select(f => new FeatureFlagFilterModel {
                    Id = f.Id,
                    FilterType = (Constants.FilterTypes)f.FilterType,
                    // Targeting
                    TargetUsers = f.Users.Where(u => u.Include).Select(u => u.User).ToList(),
                    ExcludeUsers = f.Users.Where(u => !u.Include).Select(u => u.User).ToList(),
                    // TimeWindow
                    TimeStart = f.TimeStart,
                    TimeEnd = f.TimeEnd,
                    TimeRecurrenceType = f.TimeRecurrenceType != null ? (RecurrencePatternType?)f.TimeRecurrenceType : null,
                    TimeRecurrenceInterval = f.TimeRecurrenceInterval,
                    TimeRecurrenceDaysOfWeek = !string.IsNullOrEmpty(f.TimeRecurrenceDaysOfWeek)
                        ? f.TimeRecurrenceDaysOfWeek.Split(',', StringSplitOptions.None).ToList()
                        : null,
                    TimeRecurrenceFirstDayOfWeek = f.TimeRecurrenceFirstDayOfWeek,
                    TimeRecurrenceRangeType = f.TimeRecurrenceRangeType != null ? (RecurrenceRangeType?)f.TimeRecurrenceRangeType : null,
                    TimeRecurrenceEndDate = f.TimeRecurrenceEndDate,
                    TimeRecurrenceNumberOfOccurrences = f.TimeRecurrenceNumberOfOccurrences,
                    // Percentage
                    PercentageValue = f.PercentageValue,
                    // JSON
                    JSON = f.JSON,
                    UpdatedDate = f.UpdatedDate
                }).ToList(),
                UpdatedDate = x.UpdatedDate
            });
}
