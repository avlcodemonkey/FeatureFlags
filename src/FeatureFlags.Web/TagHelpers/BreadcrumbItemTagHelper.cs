using FeatureFlags.Constants;
using FeatureFlags.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FeatureFlags.TagHelpers;

public sealed class BreadcrumbItemTagHelper(IHtmlHelper htmlHelper) : BaseTagHelper(htmlHelper) {
    public string? Action { get; set; }
    public string? Controller { get; set; }
    public bool Active { get; set; }
    public string Label { get; set; } = "";
    public object? RouteValues { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output) {
        Contextualize();

        if (HtmlHelper != null) {
            if (Active) {
                HtmlHelper.ViewData[ViewProperties.Title] = Label;
                output.Content.Append(Label);

                var httpContext = HtmlHelper.ViewContext.HttpContext;
                if (!string.IsNullOrWhiteSpace(Label) && httpContext.Request.Headers.Any(x => x.Key.ToLowerInvariant() == PJax.Request)) {
                    // headers have to be ascii chars only
                    httpContext.Response.Headers.Append(PJax.Title, string.Concat(Label.Where(x => x is >= (char)32 and < (char)127)));
                }
            } else {
                if (!string.IsNullOrWhiteSpace(Controller)) {
                    Controller = Controller.StripController();
                }
                output.Content.AppendHtml(HtmlHelper.ActionLink(Label, Action, Controller, RouteValues));
            }
        }

        output.TagName = "li";
        output.TagMode = TagMode.StartTagAndEndTag;

        await base.ProcessAsync(context, output);
    }
}
