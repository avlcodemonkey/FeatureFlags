using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Net.Mail;
using FeatureFlags.Resources;

namespace FeatureFlags.Validators;

/// <summary>
/// Validates email address with custom error message.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class IsEmailAttribute : ValidationAttribute {
    /// <summary>
    /// Validates whether the specified value is a valid email address.
    /// </summary>
    /// <param name="value">Value to validate. Must be a string representing an email address.</param>
    /// <param name="validationContext">Provides contextual information about the validation operation, including the member being validated.</param>
    /// <returns><see cref="ValidationResult"/> indicating the outcome of the validation.</returns>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext) {
        if (value is not string valueAsString || !MailAddress.TryCreate(valueAsString, out _)) {
            var errorMessage = string.Format(CultureInfo.CurrentCulture, Core.ErrorEmailAddress, validationContext.MemberName);
            return new ValidationResult(errorMessage, new[] { nameof(validationContext.MemberName) });
        }

        return ValidationResult.Success;
    }
}
