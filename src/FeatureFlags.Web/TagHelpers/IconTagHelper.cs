using System.Text.Encodings.Web;
using FeatureFlags.Constants;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FeatureFlags.TagHelpers;

/// <summary>
/// Renders an icon with an optional label using a partial view for the specified icon name.
/// </summary>
/// <param name="htmlHelper">HtmlHelper for rendering.</param>
public sealed class IconTagHelper(IHtmlHelper htmlHelper) : BaseTagHelper(htmlHelper) {
    /// <summary>
    /// Gets or sets the icon to render.
    /// </summary>
    public Icon Name { get; set; }

    /// <summary>
    /// Gets or sets the label text to display with the icon.
    /// </summary>
    public string Label { get; set; } = "";

    /// <summary>
    /// Gets or sets a value indicating whether the label should be visible.
    /// </summary>
    public bool ShowLabel { get; set; }

    /// <summary>
    /// Processes the tag helper and generates the icon output.
    /// </summary>
    /// <param name="context">Context for tag helper execution.</param>
    /// <param name="output">Output for tag helper content.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
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
