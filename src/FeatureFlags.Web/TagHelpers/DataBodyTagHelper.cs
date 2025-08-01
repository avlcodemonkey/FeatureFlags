using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FeatureFlags.TagHelpers;

/// <summary>
/// Renders a table body section (&lt;tbody&gt;) for a data table, supporting status display and column span configuration.
/// </summary>
/// <param name="htmlHelper">HtmlHelper for rendering.</param>
public sealed class DataBodyTagHelper(IHtmlHelper htmlHelper) : BaseTagHelper(htmlHelper) {
    /// <summary>
    /// Gets or sets the number of columns for the table.
    /// </summary>
    [HtmlAttributeName("colspan")]
    public int ColSpan { get; set; }

    /// <summary>
    /// Processes the tag helper and generates the table body output.
    /// </summary>
    /// <param name="context">Context for tag helper execution.</param>
    /// <param name="output">Output for tag helper content.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output) {
        Contextualize();

        var tr = new TagBuilder("tr");
        tr.InnerHtml.AppendHtml(await output.GetChildContentAsync());

        var template = new TagBuilder("template");
        template.InnerHtml.AppendHtml(tr);

        output.TagName = "tbody";
        output.Content.AppendHtml(await HtmlHelper!.PartialAsync("_DataTableStatus", ColSpan));
        output.Content.AppendHtml(template);

        await base.ProcessAsync(context, output);
    }
}
