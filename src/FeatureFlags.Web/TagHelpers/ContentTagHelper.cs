using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FeatureFlags.TagHelpers;

public sealed class ContentTagHelper : BaseTagHelper {
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output) {
        output.TagName = "div";
        output.Attributes.SetAttribute("id", "content");
        await base.ProcessAsync(context, output);
    }
}
