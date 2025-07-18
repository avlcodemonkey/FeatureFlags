using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FeatureFlags.TagHelpers;

/// <summary>
/// Creates an input group for a select with label.
/// </summary>
/// <param name="htmlHelper">HtmlHelper for rendering.</param>
public sealed class SelectGroupTagHelper(IHtmlHelper htmlHelper) : GroupBaseTagHelper(htmlHelper) {
    private IHtmlContent BuildInput(TagHelperAttributeList attributes) {
        if (string.IsNullOrWhiteSpace(FieldName)) {
            return HtmlString.Empty;
        }

        var input = new TagBuilder("select");
        // add any attributes passed in first. we'll overwrite ones we need as we build
        attributes.ToList().ForEach(x => input.MergeAttribute(x.Name, x.Value.ToString()));

        input.MergeAttribute("id", FieldName, true);
        input.MergeAttribute("name", FieldName, true);

        if (Required == true || (!Required.HasValue && For?.Metadata.IsRequired == true)) {
            input.MergeAttribute("required", "true", true);
        }

        var selectedValue = For?.ModelExplorer.Model?.ToString();
        input.InnerHtml.AppendHtml(new TagBuilder("option"));
        Options.ToList().ForEach(x => {
            var opt = new TagBuilder("option");
            opt.MergeAttribute("value", x.Value.Trim());
            if (selectedValue == x.Value) {
                opt.MergeAttribute("selected", "true");
            }
            opt.InnerHtml.Append(x.Text.Trim());
            input.InnerHtml.AppendHtml(opt);
        });

        return input;
    }

    public IEnumerable<SelectListItem> Options { get; set; } = Enumerable.Empty<SelectListItem>();

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output) {
        Contextualize();

        if (Options != null) {
            Options = Options.Where(x => !string.IsNullOrWhiteSpace(x.Value)).GroupBy(x => x.Value).Select(x => x.First());
        }

        var inputGroup = BuildInputGroup();
        inputGroup.InnerHtml.AppendHtml(BuildInput(output.Attributes));
        inputGroup.InnerHtml.AppendHtml(await BuildHelp());

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.Clear();
        output.Content.AppendHtml(BuildLabel());
        output.Content.AppendHtml(inputGroup);

        await base.ProcessAsync(context, output);
    }
}
