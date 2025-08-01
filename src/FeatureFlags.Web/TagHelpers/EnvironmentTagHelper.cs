using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FeatureFlags.TagHelpers;

/// <summary>
/// Sets a data-environment attribute on the element based on the current hosting environment.
/// </summary>
/// <param name="environment">Web host environment instance.</param>
[HtmlTargetElement(Attributes = "data-environment")]
public sealed class EnvironmentTagHelper(IWebHostEnvironment environment) : TagHelper {
    private readonly IWebHostEnvironment _Environment = environment;

    /// <summary>
    /// Processes the tag helper and sets the data-environment attribute.
    /// </summary>
    /// <param name="context">Context for tag helper execution.</param>
    /// <param name="output">Output for tag helper content.</param>
    public override void Process(TagHelperContext context, TagHelperOutput output) {
        var environmentName = _Environment.EnvironmentName.ToLower();
        output.Attributes.SetAttribute("data-environment", environmentName);
    }
}
