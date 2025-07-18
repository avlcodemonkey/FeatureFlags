using System.ComponentModel.DataAnnotations;
using System.Globalization;
using FeatureFlags.Resources;

namespace FeatureFlags.Validators;

/// <summary>
/// Validates the string length with custom error message.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class IsStringLengthAttribute(int maximumLength) : StringLengthAttribute(maximumLength) {
    public override string FormatErrorMessage(string name) {
        var errorMessage = string.Format(CultureInfo.CurrentCulture, Core.ErrorMaxLength, name, MaximumLength, MinimumLength);
        return ErrorMessage ?? errorMessage;
    }
}
