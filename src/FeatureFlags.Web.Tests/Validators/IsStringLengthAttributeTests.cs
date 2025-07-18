using System.Globalization;
using FeatureFlags.Validators;

namespace FeatureFlags.Web.Tests.Validators;

public class IsStringLengthAttributeTests {
    [Fact]
    public void FormatErrorMessage_ReturnsDefaultErrorMessage_WhenErrorMessageIsNull() {
        // Arrange
        var attribute = new IsStringLengthAttribute(10) { MinimumLength = 5 };
        var fieldName = "TestField";

        // Act
        var result = attribute.FormatErrorMessage(fieldName);

        // Assert
        var expectedMessage = string.Format(CultureInfo.CurrentCulture, "{0} cannot be more than {1} characters.", fieldName, 10);
        Assert.Equal(expectedMessage, result);
    }

    [Fact]
    public void FormatErrorMessage_ReturnsCustomErrorMessage_WhenErrorMessageIsNotNull() {
        // Arrange
        var attribute = new IsStringLengthAttribute(10) { ErrorMessage = "Custom error message", MinimumLength = 5 };
        var fieldName = "TestField";

        // Act
        var result = attribute.FormatErrorMessage(fieldName);

        // Assert
        Assert.Equal("Custom error message", result);
    }
}
