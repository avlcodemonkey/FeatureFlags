using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace FeatureFlags.Services;

/// <summary>
/// Renders a view to a string.
/// </summary>
/// <remarks>
/// https://weblog.west-wind.com/posts/2022/Jun/21/Back-to-Basics-Rendering-Razor-Views-to-String-in-ASPNET-Core
/// </remarks>
public sealed class ViewService : IViewService {
    /// <summary>
    /// Renders the specified view using the model and context.
    /// </summary>
    public async Task<string> RenderViewToStringAsync(string viewName, object model, ControllerContext context, bool isPartial = false) {
        var actionContext = context as ActionContext;

        var serviceProvider = context.HttpContext.RequestServices;
        if (serviceProvider.GetService(typeof(IRazorViewEngine)) is not IRazorViewEngine razorViewEngine) {
            throw new ArgumentException(nameof(razorViewEngine));
        }
        if (serviceProvider.GetService(typeof(ITempDataProvider)) is not ITempDataProvider tempDataProvider) {
            throw new ArgumentException(nameof(razorViewEngine));
        }

        var viewResult = razorViewEngine.FindView(actionContext, viewName, !isPartial);
        if (viewResult?.View == null) {
            throw new ArgumentException($"{viewName} does not match any available view.");
        }

        var viewDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary()) { Model = model };
        using var writer = new StringWriter();
        var viewContext = new ViewContext(actionContext, viewResult.View, viewDictionary,
            new TempDataDictionary(actionContext.HttpContext, tempDataProvider), writer, new HtmlHelperOptions());

        await viewResult.View.RenderAsync(viewContext);
        return writer.ToString();
    }
}
