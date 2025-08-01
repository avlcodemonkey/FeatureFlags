using System.Globalization;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FeatureFlags.TagHelpers;

/// <summary>
/// Suppresses output if user does not have specified role(s).
/// </summary>
/// <param name="httpContextAccessor">HTTP context accessor for retrieving user information.</param>
[HtmlTargetElement(Attributes = "authorize-roles")]
public sealed class AuthorizeTagHelper(IHttpContextAccessor httpContextAccessor) : TagHelper {
    private readonly IHttpContextAccessor _HttpContextAccessor = httpContextAccessor;

    /// <summary>
    /// Gets or sets a comma-separated list of roles required to render the element.
    /// </summary>
    [HtmlAttributeName("authorize-roles")]
    public string Roles { get; set; } = "";

    /// <summary>
    /// Processes the tag helper and suppresses output if user is not in any of the specified roles.
    /// </summary>
    /// <param name="context">Context for tag helper execution.</param>
    /// <param name="output">Output for tag helper content.</param>
    public override void Process(TagHelperContext context, TagHelperOutput output) {
        if (output.Attributes.TryGetAttribute("authorize-roles", out var attribute)) {
            output.Attributes.Remove(attribute);
        }

        var user = _HttpContextAccessor.HttpContext?.User;
        if (user == null || !Roles.Split(',').Select(x => x.Trim().ToLower(CultureInfo.InvariantCulture)).Any(user.IsInRole)) {
            output.SuppressOutput();
        }
    }
}
