using Microsoft.AspNetCore.Mvc;

namespace FeatureFlags.Services;

/// <summary>
/// Renders a view to a string.
/// </summary>
/// <remarks>
/// https://weblog.west-wind.com/posts/2022/Jun/21/Back-to-Basics-Rendering-Razor-Views-to-String-in-ASPNET-Core
/// </remarks>
public interface IViewService {
    /// <summary>
    /// Renders the specified view using the model and context.
    /// </summary>
    Task<string> RenderViewToStringAsync(string viewName, object model, ControllerContext context, bool isPartial = false);
}
