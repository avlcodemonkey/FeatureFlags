using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FeatureFlags.TagHelpers;

/// <summary>
/// Creates a checkbox with label.
/// </summary>
/// <param name="htmlHelper">HtmlHelper for rendering.</param>
public sealed class FormCheckboxTagHelper(IHtmlHelper htmlHelper) : BaseTagHelper(htmlHelper) {
    public string? Id { get; set; }
    public bool Checked { get; set; }
    public string Label { get; set; } = "";
    public string Name { get; set; } = "";
    public string Value { get; set; } = "";

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output) {
        Contextualize();
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        var id = string.IsNullOrWhiteSpace(Id) ? $"{Name}_{Value}" : Id;
        var label = new TagBuilder("label");
        label.MergeAttribute("for", id);

        var input = new TagBuilder("input");
        input.MergeAttribute("id", id);
        input.MergeAttribute("name", Name);
        input.MergeAttribute("type", "checkbox");
        input.MergeAttribute("value", Value);
        if (Checked) {
            input.MergeAttribute("checked", "true");
        }

        label.InnerHtml.AppendHtml(input);
        label.InnerHtml.Append(Label);

        output.Content.AppendHtml(label);

        await base.ProcessAsync(context, output);
    }
}
