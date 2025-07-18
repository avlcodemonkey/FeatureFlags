using FeatureFlags.Resources;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FeatureFlags.TagHelpers;

public sealed class BreadcrumbTagHelper(IHtmlHelper htmlHelper) : BaseTagHelper(htmlHelper) {
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output) {
        Contextualize();

        var li = new TagBuilder("li");
        li.AddCssClass("breadcrumb-item");
        li.InnerHtml.AppendHtml(HtmlHelper.ActionLink(Core.Dashboard, "Index", "Dashboard"));

        output.TagName = "ul";
        output.Attributes.SetAttribute("id", "breadcrumb");
        output.Content.AppendHtml(li);
        output.Content.AppendHtml(await output.GetChildContentAsync());

        await base.ProcessAsync(context, output);
    }
}
