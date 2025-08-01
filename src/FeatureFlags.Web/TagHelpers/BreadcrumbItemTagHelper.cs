using FeatureFlags.Constants;
using FeatureFlags.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FeatureFlags.TagHelpers;

/// <summary>
/// Renders a breadcrumb navigation item, supporting active and linked states.
/// </summary>
/// <param name="htmlHelper">HtmlHelper for rendering.</param>
public sealed class BreadcrumbItemTagHelper(IHtmlHelper htmlHelper) : BaseTagHelper(htmlHelper) {
    /// <summary>
    /// Gets or sets the action name for the breadcrumb link.
    /// </summary>
    public string? Action { get; set; }

    /// <summary>
    /// Gets or sets the controller name for the breadcrumb link.
    /// </summary>
    public string? Controller { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the breadcrumb item is active.
    /// </summary>
    public bool Active { get; set; }

    /// <summary>
    /// Gets or sets the label text for the breadcrumb item.
    /// </summary>
    public string Label { get; set; } = "";

    /// <summary>
    /// Gets or sets the route values for the breadcrumb link.
    /// </summary>
    public object? RouteValues { get; set; }

    /// <summary>
    /// Processes the tag helper and generates the breadcrumb item output.
    /// </summary>
    /// <param name="context">Context for tag helper execution.</param>
    /// <param name="output">Output for tag helper content.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output) {
        Contextualize();

        if (HtmlHelper != null) {
            if (Active) {
                HtmlHelper.ViewData[ViewProperties.Title] = Label;
                output.Content.Append(Label);

                var httpContext = HtmlHelper.ViewContext.HttpContext;
                if (!string.IsNullOrWhiteSpace(Label) && httpContext.Request.Headers.Any(x => x.Key.ToLowerInvariant() == PJax.Request)) {
                    // headers have to be ascii chars only
                    httpContext.Response.Headers.Append(PJax.Title, string.Concat(Label.Where(x => x is >= (char)32 and < (char)127)));
                }
            } else {
                if (!string.IsNullOrWhiteSpace(Controller)) {
                    Controller = Controller.StripController();
                }
                output.Content.AppendHtml(HtmlHelper.ActionLink(Label, Action, Controller, RouteValues));
            }
        }

        output.TagName = "li";
        output.TagMode = TagMode.StartTagAndEndTag;

        await base.ProcessAsync(context, output);
    }
}
