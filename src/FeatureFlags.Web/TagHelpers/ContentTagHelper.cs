using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FeatureFlags.TagHelpers;

/// <summary>
/// Renders a div element with id "content" for page content placement.
/// </summary>
public sealed class ContentTagHelper : BaseTagHelper {
    /// <summary>
    /// Processes the tag helper and generates the content div output.
    /// </summary>
    /// <param name="context">Context for tag helper execution.</param>
    /// <param name="output">Output for tag helper content.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output) {
        output.TagName = "div";
        output.Attributes.SetAttribute("id", "content");
        await base.ProcessAsync(context, output);
    }
}
