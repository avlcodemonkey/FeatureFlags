using System.ComponentModel.DataAnnotations;
using System.Globalization;
using FeatureFlags.Resources;

namespace FeatureFlags.Validators;

/// <summary>
/// Validates the string length with custom error message.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class IsStringLengthAttribute(int maximumLength) : StringLengthAttribute(maximumLength) {
    /// <summary>
    /// Formats the error message to include the specified field name and length constraints.
    /// </summary>
    /// <param name="name">Name of the field associated with the error.</param>
    /// <returns>Formatted error message string that includes the field name and the minimum and maximum length constraints.</returns>
    public override string FormatErrorMessage(string name) {
        var errorMessage = string.Format(CultureInfo.CurrentCulture, Core.ErrorMaxLength, name, MaximumLength, MinimumLength);
        return ErrorMessage ?? errorMessage;
    }
}
