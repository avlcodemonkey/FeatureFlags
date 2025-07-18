using System.ComponentModel.DataAnnotations;
using FeatureFlags.Validators;

namespace FeatureFlags.Web.Tests.Validators;

public class IsEmailAttributeTests {
    [Fact]
    public void IsValid_ReturnsValidationResultError_WhenValueIsNull() {
        // Arrange
        var attribute = new IsEmailAttribute();
        var validationContext = new ValidationContext(new { }) { MemberName = "Email" };

        // Act
        var result = attribute.GetValidationResult(null, validationContext);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Email is not a valid email address.", result.ErrorMessage);
    }

    [Fact]
    public void IsValid_ReturnsValidationResultError_WhenValueIsNotString() {
        // Arrange
        var attribute = new IsEmailAttribute();
        var validationContext = new ValidationContext(new { }) { MemberName = "Email2" };

        // Act
        var result = attribute.GetValidationResult(123, validationContext);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Email2 is not a valid email address.", result.ErrorMessage);
    }

    [Fact]
    public void IsValid_ReturnsValidationResultError_WhenValueIsInvalidEmail() {
        // Arrange
        var attribute = new IsEmailAttribute();
        var validationContext = new ValidationContext(new { }) { MemberName = "Email3" };

        // Act
        var result = attribute.GetValidationResult("invalid-email", validationContext);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Email3 is not a valid email address.", result.ErrorMessage);
    }

    [Fact]
    public void IsValid_ReturnsSuccess_WhenValueIsValidEmail() {
        // Arrange
        var attribute = new IsEmailAttribute();
        var validationContext = new ValidationContext(new { });

        // Act
        var result = attribute.GetValidationResult("test@example.com", validationContext);

        // Assert
        Assert.Equal(ValidationResult.Success, result);
    }
}
