using System.Globalization;
using FeatureFlags.Resources;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FeatureFlags.TagHelpers;

/// <summary>
/// Creates an input group for an autocomplete with label.
/// </summary>
/// <param name="htmlHelper">HtmlHelper for rendering.</param>
public sealed class AutocompleteGroupTagHelper(IHtmlHelper htmlHelper) : GroupBaseTagHelper(htmlHelper) {
    /// <summary>
    /// Gets the name for the autocomplete input element.
    /// </summary>
    private string AutoCompleteName => $"{FieldName}_AutoComplete";

    /// <summary>
    /// Builds the autocomplete input element with appropriate attributes.
    /// </summary>
    /// <param name="attributes">Attributes to apply to the input element.</param>
    /// <returns>HTML content representing the autocomplete input.</returns>
    private IHtmlContent BuildInput(TagHelperAttributeList attributes) {
        if (string.IsNullOrWhiteSpace(FieldName)) {
            return HtmlString.Empty;
        }

        var input = new TagBuilder("input");
        // add any attributes passed in first. we'll overwrite ones we need as we build
        attributes.ToList().ForEach(x => input.MergeAttribute(x.Name, x.Value.ToString()));

        input.MergeAttribute("id", AutoCompleteName, true);
        input.MergeAttribute("name", AutoCompleteName, true);
        input.MergeAttribute("type", "text", true);
        input.MergeAttribute("placeholder", Core.StartTyping, true);
        input.MergeAttribute("autocomplete", "off", true);
        input.MergeAttribute("data-autocomplete-display", "", true);

        return input;
    }

    /// <summary>
    /// Builds the hidden input element to store the selected value.
    /// </summary>
    /// <returns>TagBuilder representing the hidden input element.</returns>
    private TagBuilder BuildHidden() {
        var input = new TagBuilder("input");
        input.MergeAttribute("id", FieldName);
        input.MergeAttribute("name", FieldName);
        input.MergeAttribute("type", "hidden");
        input.MergeAttribute("value", For?.ModelExplorer.Model?.ToString());
        input.MergeAttribute("data-autocomplete-value", "");

        if (Required == true || (!Required.HasValue && For?.Metadata.IsRequired == true)) {
            input.MergeAttribute("required", "true");
        }

        if (For != null) {
            var maxLength = GetMaxLength(For.ModelExplorer.Metadata.ValidatorMetadata);
            if (maxLength > 0) {
                input.MergeAttribute("maxlength", maxLength.ToString(CultureInfo.InvariantCulture));
            }
            var minLength = GetMinLength(For.ModelExplorer.Metadata.ValidatorMetadata);
            if (minLength > 0) {
                input.MergeAttribute("minLength", minLength.ToString(CultureInfo.InvariantCulture));
            }
        }

        return input;
    }

    /// <summary>
    /// Gets or sets the URL to fetch autocomplete data.
    /// </summary>
    public string SrcUrl { get; set; } = "";

    /// <summary>
    /// Processes the tag helper and generates the autocomplete group output.
    /// </summary>
    /// <param name="context">Context for tag helper execution.</param>
    /// <param name="output">Output for tag helper content.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output) {
        Contextualize();

        var inputGroup = BuildInputGroup();
        inputGroup.InnerHtml.AppendHtml(BuildInput(output.Attributes));
        inputGroup.InnerHtml.AppendHtml(await BuildHelp());
        inputGroup.InnerHtml.AppendHtml(output.GetChildContentAsync().Result);

        var autocomplete = new TagBuilder("nilla-autocomplete");
        if (!string.IsNullOrWhiteSpace(SrcUrl)) {
            autocomplete.MergeAttribute("data-src-url", SrcUrl);
        }
        autocomplete.MergeAttribute("data-empty-message", Core.AutocompleteNoMatches);
        autocomplete.InnerHtml.AppendHtml(BuildLabel(AutoCompleteName));
        autocomplete.InnerHtml.AppendHtml(inputGroup);
        autocomplete.InnerHtml.AppendHtml(BuildHidden());

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.Clear();
        output.Content.AppendHtml(autocomplete);

        await base.ProcessAsync(context, output);
    }
}
