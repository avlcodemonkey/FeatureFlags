using FeatureFlags.Utils;

namespace FeatureFlags.Web.Tests.Utils;

public class NameHelperTests {
    [Fact]
    public void DisplayName_WithNameEmail_ReturnsExpectedName() {
        // arrange
        var name = "name";
        var email = "a@b.com";

        // act
        var result = NameHelper.DisplayName(name, email);

        // assert
        Assert.Equal("name (a@b.com)", result);
    }

    [Fact]
    public void DisplayName_WithWhitespace_ReturnsTrimmedName() {
        // arrange
        var name = "   name   ";
        var email = "   a@b.com   ";

        // act
        var result = NameHelper.DisplayName(name, email);

        // assert
        Assert.Equal("name (a@b.com)", result);
    }

    [Fact]
    public void DisplayName_WithNoName_ReturnsEmail() {
        // arrange
        var email = "  a@b.com  ";

        // act
        var result = NameHelper.DisplayName(null, email);

        // assert
        Assert.Equal("a@b.com", result);
    }

    [Fact]
    public void DisplayName_WithNoEmail_ReturnsName() {
        // arrange
        var name = "  name  ";

        // act
        var result = NameHelper.DisplayName(name, null);

        // assert
        Assert.Equal("name", result);
    }

    [Fact]
    public void DisplayName_WithNoNameOrEmail_ReturnsEmptyString() {
        // arrange

        // act
        var result = NameHelper.DisplayName(null, null);

        // assert
        Assert.Equal("", result);
    }
}
