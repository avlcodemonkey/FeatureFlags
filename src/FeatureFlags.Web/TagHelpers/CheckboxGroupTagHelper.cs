using FeatureFlags.Extensions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FeatureFlags.TagHelpers;

/// <summary>
/// Creates an input group for a checkbox with label.
/// </summary>
/// <param name="htmlHelper">HtmlHelper for rendering.</param>
public sealed class CheckboxGroupTagHelper(IHtmlHelper htmlHelper) : GroupBaseTagHelper(htmlHelper) {
    private IHtmlContent BuildCheckbox(TagHelperAttributeList attributes) {
        if (string.IsNullOrWhiteSpace(FieldName)) {
            return HtmlString.Empty;
        }

        var label = new TagBuilder("label");
        label.MergeAttribute("for", FieldName);

        var input = new TagBuilder("input");

        // add any attributes passed in first. we'll overwrite ones we need as we build
        attributes.ToList().ForEach(x => input.MergeAttribute(x.Name, x.Value.ToString()));

        input.MergeAttribute("id", FieldName, true);
        input.MergeAttribute("name", FieldName, true);
        input.MergeAttribute("type", "checkbox", true);
        input.MergeAttribute("value", "true", true);
        if (For?.ModelExplorer.Model?.ToString().ToBool() == true) {
            input.MergeAttribute("checked", "true", true);
        }

        label.InnerHtml.AppendHtml(input);
        if (!string.IsNullOrWhiteSpace(FieldTitle)) {
            label.InnerHtml.Append(FieldTitle);
        }

        return label;
    }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output) {
        Contextualize();

        var inputGroup = BuildInputGroup();
        inputGroup.InnerHtml.AppendHtml(BuildCheckbox(output.Attributes));
        inputGroup.InnerHtml.AppendHtml(await BuildHelp());

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.Clear();
        output.Content.AppendHtml(BuildLabel(""));
        output.Content.AppendHtml(inputGroup);

        await base.ProcessAsync(context, output);
    }
}
