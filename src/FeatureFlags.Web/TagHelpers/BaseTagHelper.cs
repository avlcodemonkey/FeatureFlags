using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FeatureFlags.TagHelpers;

public class BaseTagHelper : TagHelper {
    public BaseTagHelper() { }

    public BaseTagHelper(IHtmlHelper htmlHelper) => HtmlHelper = htmlHelper;

    [HtmlAttributeNotBound]
    public IHtmlHelper? HtmlHelper { get; set; }

    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext? ViewContext { get; set; }

    public void Contextualize() {
        if (HtmlHelper != null && ViewContext != null) {
            (HtmlHelper as IViewContextAware)!.Contextualize(ViewContext);
        }
    }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output) => await base.ProcessAsync(context, output);
}
