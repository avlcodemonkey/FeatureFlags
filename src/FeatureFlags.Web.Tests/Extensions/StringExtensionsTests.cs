using FeatureFlags.Extensions;

namespace FeatureFlags.Web.Tests.Extensions;

public class StringExtensionsTests {
    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("gibberish", false)]
    [InlineData("false", false)]
    [InlineData("False", false)]
    [InlineData("0", false)]
    [InlineData("true", true)]
    [InlineData("True", true)]
    [InlineData("1", true)]
    public void ToBool_ReturnsExpectedResult(string? val, bool expected) {
        // arrange

        // act
        var result = val.ToBool();

        // assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Test", "Test")]
    [InlineData("test", "test")]
    [InlineData("TestController", "Test")]
    [InlineData("testcontroller", "test")]
    [InlineData("", "")]
    public void StripController_ReturnsExpectedValue(string controller, string expected) {
        // arrange

        // act
        var result = controller.StripController();

        // assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Test", "Test")]
    [InlineData("test", "test")]
    [InlineData("TestModel", "Test")]
    [InlineData("testmodel", "test")]
    [InlineData("", "")]
    public void StripModel_ReturnsExpectedValue(string model, string expected) {
        // arrange

        // act
        var result = model.StripModel();

        // assert
        Assert.Equal(expected, result);
    }
}
