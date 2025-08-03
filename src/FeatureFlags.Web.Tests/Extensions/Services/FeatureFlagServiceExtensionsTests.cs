using FeatureFlags.Domain.Models;
using FeatureFlags.Extensions.Services;

namespace FeatureFlags.Web.Tests.Extensions.Services;

public class FeatureFlagServiceExtensionsTests {
    [Fact]
    public void SelectSingleAsModel_ReturnsProjectedModel() {
        // arrange
        var featureFlag = new FeatureFlag {
            Id = 1, Name = "test", Status = true, CreatedDate = DateTime.MinValue, UpdatedDate = DateTime.MinValue
        };
        var featureFlags = new List<FeatureFlag> { featureFlag }.AsQueryable();

        // act
        var models = featureFlags.SelectAsModel().ToList();

        // assert
        Assert.NotNull(models);
        var singleModel = Assert.Single(models);

        Assert.Equal(featureFlag.Id, singleModel.Id);
        Assert.Equal(featureFlag.Name, singleModel.Name);
        Assert.Equal(featureFlag.Status, singleModel.Status);
        Assert.Equal(featureFlag.UpdatedDate, singleModel.UpdatedDate);
    }

    [Fact]
    public void SelectMultipleAsModel_ReturnsProjectedModels() {
        // arrange
        var featureFlag1 = new FeatureFlag {
            Id = 1, Name = "test", Status = true, CreatedDate = DateTime.MinValue, UpdatedDate = DateTime.MinValue
        };
        var featureFlag2 = new FeatureFlag {
            Id = 2, Name = "flag 2", Status = false, CreatedDate = DateTime.MaxValue, UpdatedDate = DateTime.MaxValue
        };
        var featureFlags = new List<FeatureFlag> { featureFlag1, featureFlag2 }.AsQueryable();

        // act
        var models = featureFlags.SelectAsModel().ToList();

        // assert
        Assert.NotNull(models);
        Assert.Equal(2, models.Count);

        Assert.Collection(models,
            x => Assert.Equal(featureFlag1.Id, x.Id),
            x => Assert.Equal(featureFlag2.Id, x.Id)
        );
        Assert.Collection(models,
            x => Assert.Equal(featureFlag1.Name, x.Name),
            x => Assert.Equal(featureFlag2.Name, x.Name)
        );
        Assert.Collection(models,
            x => Assert.Equal(featureFlag1.Status, x.Status),
            x => Assert.Equal(featureFlag2.Status, x.Status)
        );
        Assert.Collection(models,
            x => Assert.Equal(featureFlag1.UpdatedDate, x.UpdatedDate),
            x => Assert.Equal(featureFlag2.UpdatedDate, x.UpdatedDate)
        );
    }
}
