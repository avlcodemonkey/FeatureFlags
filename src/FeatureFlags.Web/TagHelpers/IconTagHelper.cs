using System.Text.Encodings.Web;
using FeatureFlags.Constants;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FeatureFlags.TagHelpers;

public sealed class IconTagHelper(IHtmlHelper htmlHelper) : BaseTagHelper(htmlHelper) {
    public Icon Name { get; set; }

    public string Label { get; set; } = "";

    public bool ShowLabel { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output) {
        Contextualize();

        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.AddClass("icon", HtmlEncoder.Default);
        output.Content.AppendHtml(await HtmlHelper!.PartialAsync($"Icons/_{Name}"));
        if (!string.IsNullOrWhiteSpace(Label)) {
            var label = new TagBuilder("span");
            label.InnerHtml.Append(Label);
            if (!ShowLabel) {
                label.AddCssClass("is-visually-hidden");
            }
            output.Content.AppendHtml(label);
        }

        await base.ProcessAsync(context, output);
    }
}
