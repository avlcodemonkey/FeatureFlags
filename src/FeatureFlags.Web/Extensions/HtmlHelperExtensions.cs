using System.Reflection;
using FeatureFlags.Constants;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FeatureFlags.Extensions;

public static class HtmlHelperExtensions {
    /// <summary>
    /// Format the app version in a user friendly string.
    /// </summary>
    /// <returns>App name and current version.</returns>
    public static string ApplicationVersionName(this IHtmlHelper _) {
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;

        if (version != null && assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), true).FirstOrDefault() is AssemblyProductAttribute product) {
            return $"{product.Product} v{version}";
        } else {
            return "";
        }
    }

    /// <summary>
    /// Format the app version in a user friendly string.
    /// </summary>
    /// <returns>App name and current version.</returns>
    public static string? ApplicationVersionNumber(this IHtmlHelper _)
        => Assembly.GetExecutingAssembly().GetName().Version?.ToString();

    /// <summary>
    /// Get the width as the correct type.
    /// </summary>
    /// <remarks>Used with `table` `width` attribute to set column pixel width and prevent weird redraws when sorting table.</remarks>
    /// <returns>Numeric width.</returns>
    public static int Width(this IHtmlHelper _, ColumnWidth columnWidth) => (int)columnWidth;
}
