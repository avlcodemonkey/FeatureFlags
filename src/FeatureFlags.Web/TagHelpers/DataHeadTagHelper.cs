using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FeatureFlags.TagHelpers;

/// <summary>
/// Renders a table header section (&lt;thead&gt;) for a data table, wrapping child content in a &lt;tr&gt; element.
/// </summary>
public sealed class DataHeadTagHelper : BaseTagHelper {
    /// <summary>
    /// Initializes a new instance of the <see cref="DataHeadTagHelper"/> class.
    /// </summary>
    public DataHeadTagHelper() { }

    /// <summary>
    /// Processes the tag helper and generates the table header output.
    /// </summary>
    /// <param name="context">Context for tag helper execution.</param>
    /// <param name="output">Output for tag helper content.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output) {
        var tr = new TagBuilder("tr");
        tr.InnerHtml.AppendHtml(await output.GetChildContentAsync());

        output.TagName = "thead";
        output.Content.AppendHtml(tr);

        await base.ProcessAsync(context, output);
    }
}
