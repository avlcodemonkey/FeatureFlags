using System.ComponentModel.DataAnnotations;
using System.Globalization;
using FeatureFlags.Resources;

namespace FeatureFlags.Validators;

/// <summary>
/// Required validator with custom error message.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class IsRequiredAttribute : RequiredAttribute {
    /// <summary>
    /// Formats the error message to include the specified field name.
    /// </summary>
    /// <param name="name">Name of the field associated with the error.</param>
    /// <returns>Localized error message string that includes the specified field name.</returns>
    public override string FormatErrorMessage(string name) {
        var errorMessage = string.Format(CultureInfo.CurrentCulture, Core.ErrorRequired, name);
        return ErrorMessage ?? errorMessage;
    }
}
