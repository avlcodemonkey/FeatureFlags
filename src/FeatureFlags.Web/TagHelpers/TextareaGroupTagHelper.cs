using System.Globalization;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FeatureFlags.TagHelpers;

/// <summary>
/// Creates an input group for a textarea with label.
/// </summary>
/// <param name="htmlHelper">HtmlHelper for rendering.</param>
public sealed class TextareaGroupTagHelper(IHtmlHelper htmlHelper) : GroupBaseTagHelper(htmlHelper) {
    /// <summary>
    /// Builds the textarea input element with appropriate attributes.
    /// </summary>
    /// <param name="attributes">Attributes to apply to the textarea element.</param>
    /// <returns>HTML content representing the textarea input.</returns>
    private IHtmlContent BuildInput(TagHelperAttributeList attributes) {
        if (string.IsNullOrWhiteSpace(FieldName)) {
            return HtmlString.Empty;
        }

        var textarea = new TagBuilder("textarea");
        // add any attributes passed in first. we'll overwrite ones we need as we build
        attributes.ToList().ForEach(x => textarea.MergeAttribute(x.Name, x.Value.ToString()));

        textarea.MergeAttribute("id", FieldName, true);
        textarea.MergeAttribute("name", FieldName, true);

        if (Required == true || (!Required.HasValue && For?.Metadata.IsRequired == true)) {
            textarea.MergeAttribute("required", "true", true);
        }

        if (For != null) {
            var maxLength = GetMaxLength(For.ModelExplorer.Metadata.ValidatorMetadata);
            if (maxLength > 0) {
                textarea.MergeAttribute("maxlength", maxLength.ToString(CultureInfo.InvariantCulture), true);
            }
            var minLength = GetMinLength(For.ModelExplorer.Metadata.ValidatorMetadata);
            if (minLength > 0) {
                textarea.MergeAttribute("minLength", minLength.ToString(CultureInfo.InvariantCulture), true);
            }
        }

        textarea.InnerHtml.Append(For?.ModelExplorer.Model?.ToString() ?? "");
        return textarea;
    }

    /// <summary>
    /// Processes the tag helper and generates the textarea group output.
    /// </summary>
    /// <param name="context">Context for the tag helper execution.</param>
    /// <param name="output">Output for the tag helper content.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output) {
        Contextualize();

        var inputGroup = BuildInputGroup();
        inputGroup.InnerHtml.AppendHtml(BuildInput(output.Attributes));
        inputGroup.InnerHtml.AppendHtml(await BuildHelp());
        inputGroup.InnerHtml.AppendHtml(output.GetChildContentAsync().Result);

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.Clear();
        output.Content.AppendHtml(BuildLabel());
        output.Content.AppendHtml(inputGroup);

        await base.ProcessAsync(context, output);
    }
}
