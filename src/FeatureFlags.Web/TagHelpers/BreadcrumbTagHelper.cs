using FeatureFlags.Resources;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FeatureFlags.TagHelpers;

/// <summary>
/// Renders a breadcrumb navigation list with a dashboard link as the first item.
/// </summary>
/// <param name="htmlHelper">HtmlHelper for rendering.</param>
public sealed class BreadcrumbTagHelper(IHtmlHelper htmlHelper) : BaseTagHelper(htmlHelper) {
    /// <summary>
    /// Processes the tag helper and generates the breadcrumb output.
    /// </summary>
    /// <param name="context">Context for tag helper execution.</param>
    /// <param name="output">Output for tag helper content.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output) {
        Contextualize();

        var li = new TagBuilder("li");
        li.AddCssClass("breadcrumb-item");
        li.InnerHtml.AppendHtml(HtmlHelper.ActionLink(Core.Dashboard, "Index", "Dashboard"));

        output.TagName = "ul";
        output.Attributes.SetAttribute("id", "breadcrumb");
        output.Content.AppendHtml(li);
        output.Content.AppendHtml(await output.GetChildContentAsync());

        await base.ProcessAsync(context, output);
    }
}
