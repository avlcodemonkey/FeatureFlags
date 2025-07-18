using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FeatureFlags.TagHelpers;

public sealed class DataHeadTagHelper : BaseTagHelper {
    public DataHeadTagHelper() { }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output) {
        var tr = new TagBuilder("tr");
        tr.InnerHtml.AppendHtml(await output.GetChildContentAsync());

        output.TagName = "thead";
        output.Content.AppendHtml(tr);

        await base.ProcessAsync(context, output);
    }
}
