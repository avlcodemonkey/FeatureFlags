using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FeatureFlags.TagHelpers;

/// <summary>
/// Base class for tag helpers that require access to <see cref="IHtmlHelper"/> and <see cref="ViewContext"/>.
/// </summary>
public class BaseTagHelper : TagHelper {
    /// <summary>
    /// Initializes a new instance of the <see cref="BaseTagHelper"/> class.
    /// </summary>
    public BaseTagHelper() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseTagHelper"/> class with specified <see cref="IHtmlHelper"/>.
    /// </summary>
    /// <param name="htmlHelper">HtmlHelper for rendering.</param>
    public BaseTagHelper(IHtmlHelper htmlHelper) => HtmlHelper = htmlHelper;

    /// <summary>
    /// Gets or sets the HTML helper for rendering views.
    /// </summary>
    [HtmlAttributeNotBound]
    public IHtmlHelper? HtmlHelper { get; set; }

    /// <summary>
    /// Gets or sets the view context for the tag helper.
    /// </summary>
    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext? ViewContext { get; set; }

    /// <summary>
    /// Contextualizes the HTML helper with the current view context.
    /// </summary>
    public void Contextualize() {
        if (HtmlHelper != null && ViewContext != null) {
            (HtmlHelper as IViewContextAware)!.Contextualize(ViewContext);
        }
    }

    /// <summary>
    /// Processes the tag helper asynchronously.
    /// </summary>
    /// <param name="context">Context for tag helper execution.</param>
    /// <param name="output">Output for tag helper content.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output) => await base.ProcessAsync(context, output);
}
