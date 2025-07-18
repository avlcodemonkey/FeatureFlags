using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FeatureFlags.TagHelpers;

public sealed class DataBodyTagHelper(IHtmlHelper htmlHelper) : BaseTagHelper(htmlHelper) {
    /// <summary>
    /// Number of columns for the table.
    /// </summary>
    [HtmlAttributeName("colspan")]
    public int ColSpan { get; set; }

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
