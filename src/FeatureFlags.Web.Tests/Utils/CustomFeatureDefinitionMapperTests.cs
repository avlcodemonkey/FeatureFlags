using FeatureFlags.Constants;
using FeatureFlags.Models;
using FeatureFlags.Utils;
using Microsoft.FeatureManagement;

namespace FeatureFlags.Web.Tests.Utils;

public class CustomFeatureDefinitionMapperTests {
    [Fact]
    public void MapToCustomFeatureDefinition_DisabledFlag_ReturnsDisabledStatus() {
        var flag = new FeatureFlagModel {
            Name = "TestFeature",
            Status = false,
            RequirementType = Constants.RequirementType.Any
        };

        var result = CustomFeatureDefinitionMapper.MapToCustomFeatureDefinition(flag);

        Assert.Equal("TestFeature", result.Name);
        Assert.Equal(FeatureStatus.Disabled, result.Status);
        Assert.Equal(Microsoft.FeatureManagement.RequirementType.Any, result.RequirementType);
        Assert.Empty(result.EnabledFor);
    }

    [Fact]
    public void MapToCustomFeatureDefinition_EnabledFlag_NoFilters_ReturnsAlwaysOn() {
        var flag = new FeatureFlagModel {
            Name = "TestFeature",
            Status = true,
            RequirementType = Constants.RequirementType.All
        };

        var result = CustomFeatureDefinitionMapper.MapToCustomFeatureDefinition(flag);

        Assert.Equal("TestFeature", result.Name);
        Assert.Equal(FeatureStatus.Conditional, result.Status);
        Assert.Single(result.EnabledFor);
        Assert.Equal("AlwaysOn", result.EnabledFor.First().Name);
    }

    [Fact]
    public void MapToCustomFeatureDefinition_EnabledFlag_PercentageFilter_ReturnsPercentageConfig() {
        var flag = new FeatureFlagModel {
            Name = "TestFeature",
            Status = true,
            RequirementType = Constants.RequirementType.All,
            Filters = new[]
            {
                new FeatureFlagFilterModel
                {
                    FilterType = FilterTypes.Percentage,
                    PercentageValue = 42
                }
            }
        };

        var result = CustomFeatureDefinitionMapper.MapToCustomFeatureDefinition(flag);

        Assert.Equal(FeatureStatus.Conditional, result.Status);
        var filter = result.EnabledFor.Single();
        Assert.Equal("Microsoft.Percentage", filter.Name);
        Assert.Contains(filter.Parameters, p => p.Key == "Value" && p.Value == "42");
    }

    [Fact]
    public void MapToCustomFeatureDefinition_EnabledFlag_TimeWindowFilter_ReturnsTimeWindowConfig() {
        var start = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = new DateTime(2025, 1, 2, 0, 0, 0, DateTimeKind.Utc);

        var flag = new FeatureFlagModel {
            Name = "TestFeature",
            Status = true,
            RequirementType = Constants.RequirementType.All,
            Filters = new[]
            {
                new FeatureFlagFilterModel
                {
                    FilterType = FilterTypes.TimeWindow,
                    TimeStart = start,
                    TimeEnd = end
                }
            }
        };

        var result = CustomFeatureDefinitionMapper.MapToCustomFeatureDefinition(flag);

        var filter = result.EnabledFor.Single();
        Assert.Equal("Microsoft.TimeWindow", filter.Name);
        Assert.Contains(filter.Parameters, p => p.Key == "Start" && p.Value != null);
        Assert.Contains(filter.Parameters, p => p.Key == "End" && p.Value != null);
    }

    [Fact]
    public void MapToCustomFeatureDefinition_EnabledFlag_TargetingFilter_ReturnsTargetingConfig() {
        var flag = new FeatureFlagModel {
            Name = "TestFeature",
            Status = true,
            RequirementType = Constants.RequirementType.All,
            Filters = new[]
            {
                new FeatureFlagFilterModel
                {
                    FilterType = FilterTypes.Targeting,
                    TargetUsers = new[] { "user1", "user2" },
                    ExcludeUsers = new[] { "user3" }
                }
            }
        };

        var result = CustomFeatureDefinitionMapper.MapToCustomFeatureDefinition(flag);

        var filter = result.EnabledFor.Single();
        Assert.Equal("Microsoft.Targeting", filter.Name);
        Assert.Contains(filter.Parameters, p => p.Key.StartsWith("Audience:Users") && p.Value?.Contains("user1") == true);
        Assert.Contains(filter.Parameters, p => p.Key.StartsWith("Audience:Exclusion:Users") && p.Value?.Contains("user3") == true);
    }

    [Fact]
    public void MapToCustomFeatureDefinition_EnabledFlag_JsonFilter_ReturnsJsonConfig() {
        var json = "{ \"name\": \"CustomFilter\", \"parameters\": { \"foo\": \"bar\" } }";
        var flag = new FeatureFlagModel {
            Name = "TestFeature",
            Status = true,
            RequirementType = Constants.RequirementType.All,
            Filters = new[]
            {
                new FeatureFlagFilterModel
                {
                    FilterType = FilterTypes.JSON,
                    JSON = json
                }
            }
        };

        var result = CustomFeatureDefinitionMapper.MapToCustomFeatureDefinition(flag);

        var filter = result.EnabledFor.Single();
        Assert.Equal("CustomFilter", filter.Name);
        Assert.Contains(filter.Parameters, p => p.Key == "parameters:foo" && p.Value == "bar");
    }

    [Fact]
    public void MapToCustomFeatureDefinition_EnabledFlag_JsonFilter_EmptyJson_ReturnsNoConfig() {
        var flag = new FeatureFlagModel {
            Name = "TestFeature",
            Status = true,
            RequirementType = Constants.RequirementType.All,
            Filters = new[]
            {
                new FeatureFlagFilterModel
                {
                    FilterType = FilterTypes.JSON,
                    JSON = ""
                }
            }
        };

        var result = CustomFeatureDefinitionMapper.MapToCustomFeatureDefinition(flag);

        // Should fallback to AlwaysOn since JSON filter is ignored
        Assert.Single(result.EnabledFor);
        Assert.Equal("AlwaysOn", result.EnabledFor.First().Name);
    }
}
