using System.ComponentModel.DataAnnotations;
using System.Globalization;
using FeatureFlags.Constants;
using FeatureFlags.Extensions;
using FeatureFlags.Resources;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FeatureFlags.TagHelpers;

/// <summary>
/// Provides helpers for creating input groups. Should not be used directly.
/// </summary>
/// <param name="htmlHelper">HtmlHelper for rendering.</param>
public class GroupBaseTagHelper(IHtmlHelper htmlHelper) : BaseTagHelper(htmlHelper) {
    /// <summary>
    /// Gets the field name for the input group.
    /// </summary>
    public string? FieldName => For?.Name ?? Name;

    /// <summary>
    /// Gets the field title for the input group.
    /// </summary>
    public string? FieldTitle => For?.Metadata.DisplayName ?? Title;

    /// <summary>
    /// Gets or sets the model expression for the field.
    /// </summary>
    public ModelExpression? For { get; set; }

    /// <summary>
    /// Gets or sets the help text for the input group.
    /// </summary>
    public string? HelpText { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the field is required.
    /// </summary>
    public bool? Required { get; set; }

    /// <summary>
    /// Gets or sets the name of the field.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the title of the field.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Builds a div element with the input group CSS class.
    /// </summary>
    /// <returns>TagBuilder representing the input group container.</returns>
    public static TagBuilder BuildInputGroup() {
        var inputGroup = new TagBuilder("div");
        inputGroup.AddCssClass("input-group");
        return inputGroup;
    }

    /// <summary>
    /// Gets the maximum string length from validator metadata.
    /// </summary>
    /// <param name="validatorMetadata">Validator metadata to inspect.</param>
    /// <returns>Maximum string length if found; otherwise, 0.</returns>
    public static int GetMaxLength(IReadOnlyList<object> validatorMetadata) {
        for (var i = 0; i < validatorMetadata.Count; i++) {
            if (validatorMetadata[i] is StringLengthAttribute stringLengthAttribute && stringLengthAttribute.MaximumLength > 0) {
                return stringLengthAttribute.MaximumLength;
            }

            if (validatorMetadata[i] is MaxLengthAttribute maxLengthAttribute && maxLengthAttribute.Length > 0) {
                return maxLengthAttribute.Length;
            }
        }
        return 0;
    }

    /// <summary>
    /// Gets the minimum string length from validator metadata.
    /// </summary>
    /// <param name="validatorMetadata">Validator metadata to inspect.</param>
    /// <returns>Minimum string length if found; otherwise, 0.</returns>
    public static int GetMinLength(IReadOnlyList<object> validatorMetadata) {
        for (var i = 0; i < validatorMetadata.Count; i++) {
            if (validatorMetadata[i] is StringLengthAttribute stringLengthAttribute && stringLengthAttribute.MinimumLength > 0) {
                return stringLengthAttribute.MinimumLength;
            }

            if (validatorMetadata[i] is MinLengthAttribute minLengthAttribute && minLengthAttribute.Length > 0) {
                return minLengthAttribute.Length;
            }
        }
        return 0;
    }

    /// <summary>
    /// Builds the help button and dialog for the input group.
    /// </summary>
    /// <returns>HTML content for the help dialog.</returns>
    public async Task<IHtmlContent> BuildHelp() {
        if (HtmlHelper!.ViewContext.HttpContext?.Session.IsEnabled(SessionProperties.Help) != true) {
            return HtmlString.Empty;
        }

        if (string.IsNullOrWhiteSpace(HelpText) && For != null) {
            HelpText = ContextHelp.ResourceManager.GetString($"{For.Metadata.ContainerType!.Name.StripModel()}_{For.Metadata.PropertyName}", CultureInfo.InvariantCulture) ?? "";
        }

        if (string.IsNullOrWhiteSpace(HelpText)) {
            return HtmlString.Empty;
        }

        var button = new TagBuilder("button");
        button.AddCssClass("button success button-icon");
        button.MergeAttribute("type", "button");
        button.MergeAttribute("data-dialog-content", HelpText!.Replace("\"", "&quot;"));
        button.MergeAttribute("data-dialog-ok", Core.Okay);
        button.InnerHtml.AppendHtml(await HtmlHelper!.PartialAsync("Icons/_CircleInformation"));

        var labelSpan = new TagBuilder("span");
        labelSpan.InnerHtml.Append(Core.Help);
        labelSpan.AddCssClass("is-visually-hidden");
        button.InnerHtml.AppendHtml(labelSpan);

        var dialog = new TagBuilder("nilla-info");
        dialog.InnerHtml.AppendHtml(button);

        return dialog;
    }

    /// <summary>
    /// Builds a label element for the input group.
    /// </summary>
    /// <param name="forField">Field name to associate with the label.</param>
    /// <returns>HTML content for the label element.</returns>
    public IHtmlContent BuildLabel(string? forField = null) {
        if (string.IsNullOrWhiteSpace(FieldTitle)) {
            return HtmlString.Empty;
        }

        var label = new TagBuilder("label");
        if (!string.IsNullOrWhiteSpace(forField ?? FieldName)) {
            label.MergeAttribute("for", forField ?? FieldName);
        }
        label.InnerHtml.Append(FieldTitle);

        return label;
    }

    /// <summary>
    /// Processes the tag helper asynchronously.
    /// </summary>
    /// <param name="context">Context for tag helper execution.</param>
    /// <param name="output">Output for tag helper content.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output) => await base.ProcessAsync(context, output);
}
