using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace FeatureFlags.Services;

/// <inheritdoc />
public sealed class ViewService : IViewService {
    /// <inheritdoc />
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
