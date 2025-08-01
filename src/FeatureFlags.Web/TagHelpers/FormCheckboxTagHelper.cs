using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FeatureFlags.TagHelpers;

/// <summary>
/// Creates a checkbox with label.
/// </summary>
/// <param name="htmlHelper">HtmlHelper for rendering.</param>
public sealed class FormCheckboxTagHelper(IHtmlHelper htmlHelper) : BaseTagHelper(htmlHelper) {
    /// <summary>
    /// Gets or sets the ID attribute for the checkbox input.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the checkbox is checked.
    /// </summary>
    public bool Checked { get; set; }

    /// <summary>
    /// Gets or sets the label text for the checkbox.
    /// </summary>
    public string Label { get; set; } = "";

    /// <summary>
    /// Gets or sets the name attribute for the checkbox input.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Gets or sets the value attribute for the checkbox input.
    /// </summary>
    public string Value { get; set; } = "";

    /// <summary>
    /// Processes the tag helper and generates the checkbox output.
    /// </summary>
    /// <param name="context">Context for tag helper execution.</param>
    /// <param name="output">Output for tag helper content.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
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
