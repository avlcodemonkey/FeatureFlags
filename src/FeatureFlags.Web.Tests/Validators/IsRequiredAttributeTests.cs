using System.Globalization;
using FeatureFlags.Validators;

namespace FeatureFlags.Web.Tests.Validators;

public class IsRequiredAttributeTests {
    [Fact]
    public void FormatErrorMessage_ReturnsDefaultErrorMessage_WhenErrorMessageIsNull() {
        // Arrange
        var attribute = new IsRequiredAttribute();
        var fieldName = "TestField";

        // Act
        var result = attribute.FormatErrorMessage(fieldName);

        // Assert
        var expectedMessage = string.Format(CultureInfo.CurrentCulture, "{0} is required.", fieldName);
        Assert.Equal(expectedMessage, result);
    }

    [Fact]
    public void FormatErrorMessage_ReturnsCustomErrorMessage_WhenErrorMessageIsNotNull() {
        // Arrange
        var attribute = new IsRequiredAttribute { ErrorMessage = "Custom error message" };
        var fieldName = "TestField";

        // Act
        var result = attribute.FormatErrorMessage(fieldName);

        // Assert
        Assert.Equal("Custom error message", result);
    }
}
