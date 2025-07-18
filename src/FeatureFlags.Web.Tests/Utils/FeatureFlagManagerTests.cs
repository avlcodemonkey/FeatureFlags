using FeatureFlags.Models;
using FeatureFlags.Services;
using FeatureFlags.Utils;
using Moq;

namespace FeatureFlags.Web.Tests.Utils;

public class FeatureFlagManagerTests {
    private readonly List<FeatureFlagModel> _FeatureFlagList = [
        new FeatureFlagModel { Name = "flag1", IsEnabled = true },
        new FeatureFlagModel { Name = "flag2", IsEnabled = false },
    ];

    private readonly Dictionary<string, string> _FeatureFlagDictionary = new() {
        { "flag1", "flag1" },
        { "flag2", "flag2" },
    };

    [Fact]
    public async Task RegisterAsync_WithNoChanges_DoesNothing() {
        // arrange
        var mockFeatureFlagService = new Mock<IFeatureFlagService>();
        mockFeatureFlagService.Setup(x => x.GetAllFeatureFlagsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(_FeatureFlagList);
        mockFeatureFlagService.Setup(x => x.SaveFeatureFlagAsync(It.IsAny<FeatureFlagModel>(), It.IsAny<CancellationToken>())).ReturnsAsync((true, ""));
        mockFeatureFlagService.Setup(x => x.DeleteFeatureFlagAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var featureFlagManager = new FeatureFlagManager(mockFeatureFlagService.Object);

        // act
        var result = await featureFlagManager.RegisterAsync(_FeatureFlagDictionary);

        // assert
        Assert.True(result);
        mockFeatureFlagService.Verify(x => x.GetAllFeatureFlagsAsync(It.IsAny<CancellationToken>()), Times.Once);
        mockFeatureFlagService.Verify(x => x.SaveFeatureFlagAsync(It.IsAny<FeatureFlagModel>(), It.IsAny<CancellationToken>()), Times.Never);
        mockFeatureFlagService.Verify(x => x.DeleteFeatureFlagAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_WitNewFlagsChanges_AddsFlag() {
        // arrange
        var mockFeatureFlagService = new Mock<IFeatureFlagService>();
        mockFeatureFlagService.Setup(x => x.GetAllFeatureFlagsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(_FeatureFlagList);
        mockFeatureFlagService.Setup(x => x.SaveFeatureFlagAsync(It.IsAny<FeatureFlagModel>(), It.IsAny<CancellationToken>())).ReturnsAsync((true, ""));
        mockFeatureFlagService.Setup(x => x.DeleteFeatureFlagAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var newFlags = new Dictionary<string, string> {
            { "flag1", "flag1" },
            { "flag2", "flag2" },
            { "flag3", "flag3" },
        };
        var featureFlagManager = new FeatureFlagManager(mockFeatureFlagService.Object);

        // act
        var result = await featureFlagManager.RegisterAsync(newFlags);

        // assert
        Assert.True(result);
        mockFeatureFlagService.Verify(x => x.GetAllFeatureFlagsAsync(It.IsAny<CancellationToken>()), Times.Once);
        mockFeatureFlagService.Verify(x => x.SaveFeatureFlagAsync(It.IsAny<FeatureFlagModel>(), It.IsAny<CancellationToken>()), Times.Once);
        mockFeatureFlagService.Verify(x => x.DeleteFeatureFlagAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_WithRemovedFlag_DeletesFlag() {
        // arrange
        var mockFeatureFlagService = new Mock<IFeatureFlagService>();
        mockFeatureFlagService.Setup(x => x.GetAllFeatureFlagsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(_FeatureFlagList);
        mockFeatureFlagService.Setup(x => x.SaveFeatureFlagAsync(It.IsAny<FeatureFlagModel>(), It.IsAny<CancellationToken>())).ReturnsAsync((true, ""));
        mockFeatureFlagService.Setup(x => x.DeleteFeatureFlagAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var newFlags = new Dictionary<string, string> {
            { "flag2", "flag2" },
        };
        var featureFlagManager = new FeatureFlagManager(mockFeatureFlagService.Object);

        // act
        var result = await featureFlagManager.RegisterAsync(newFlags);

        // assert
        Assert.True(result);
        mockFeatureFlagService.Verify(x => x.GetAllFeatureFlagsAsync(It.IsAny<CancellationToken>()), Times.Once);
        mockFeatureFlagService.Verify(x => x.SaveFeatureFlagAsync(It.IsAny<FeatureFlagModel>(), It.IsAny<CancellationToken>()), Times.Never);
        mockFeatureFlagService.Verify(x => x.DeleteFeatureFlagAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
    }


    [Fact]
    public async Task RegisterAsync_WithServiceError_ReturnsFalse() {
        // arrange
        var mockFeatureFlagService = new Mock<IFeatureFlagService>();
        mockFeatureFlagService.Setup(x => x.GetAllFeatureFlagsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(_FeatureFlagList);
        mockFeatureFlagService.Setup(x => x.SaveFeatureFlagAsync(It.IsAny<FeatureFlagModel>(), It.IsAny<CancellationToken>())).ReturnsAsync((true, ""));
        // returning false here will trigger the manager to return false
        mockFeatureFlagService.Setup(x => x.DeleteFeatureFlagAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var newFlags = new Dictionary<string, string> {
            { "flag2", "flag2" },
        };
        var featureFlagManager = new FeatureFlagManager(mockFeatureFlagService.Object);

        // act
        var result = await featureFlagManager.RegisterAsync(newFlags);

        // assert
        Assert.False(result);
        mockFeatureFlagService.Verify(x => x.GetAllFeatureFlagsAsync(It.IsAny<CancellationToken>()), Times.Once);
        mockFeatureFlagService.Verify(x => x.SaveFeatureFlagAsync(It.IsAny<FeatureFlagModel>(), It.IsAny<CancellationToken>()), Times.Never);
        mockFeatureFlagService.Verify(x => x.DeleteFeatureFlagAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
