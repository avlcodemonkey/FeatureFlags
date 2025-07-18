using System.ComponentModel.DataAnnotations;
using System.Globalization;
using FeatureFlags.Resources;

namespace FeatureFlags.Validators;

/// <summary>
/// Required validator with custom error message.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class IsRequiredAttribute : RequiredAttribute {
    public override string FormatErrorMessage(string name) {
        var errorMessage = string.Format(CultureInfo.CurrentCulture, Core.ErrorRequired, name);
        return ErrorMessage ?? errorMessage;
    }
}
