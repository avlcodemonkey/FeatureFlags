using System.Text.Encodings.Web;
using FeatureFlags.Constants;
using FeatureFlags.Extensions;
using FeatureFlags.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FeatureFlags.TagHelpers;

public sealed class FormContentTagHelper(IHtmlHelper htmlHelper, IUrlHelperFactory urlHelperFactory) : BaseTagHelper(htmlHelper) {
    private readonly IUrlHelperFactory _UrlHelperFactory = urlHelperFactory;

    public string? Action { get; set; }
    public string? Controller { get; set; }
    public object? For { get; set; }
    public HttpMethod Method { get; set; } = HttpMethod.Post;
    public object? RouteValues { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output) {
        Contextualize();

        var hiddenInputs = new List<TagBuilder>();
        if (For != null) {
            Controller = string.IsNullOrWhiteSpace(Controller) ? For.GetType().Name.StripModel() : Controller;

            // if the model is versioned, create the hidden input for the rowversion field
            if (For is IVersionedModel versionedModel) {
                var rowVersionInput = new TagBuilder("input");
                rowVersionInput.MergeAttribute("type", "hidden");
                rowVersionInput.MergeAttribute("name", nameof(IVersionedModel.UpdatedDate));
                rowVersionInput.MergeAttribute("value", versionedModel.UpdatedDate.ToString());
                hiddenInputs.Add(rowVersionInput);
            }

            // if the model is an audited model, create the hidden input for the id field
            if (For is IAuditedModel auditedModel) {
                Method = auditedModel.Id == 0 ? HttpMethod.Post : HttpMethod.Put;

                var idInput = new TagBuilder("input");
                idInput.MergeAttribute("type", "hidden");
                idInput.MergeAttribute("name", nameof(IAuditedModel.Id));
                idInput.MergeAttribute("value", auditedModel.Id.ToString());
                hiddenInputs.Add(idInput);
            }
        }

        output.TagMode = TagMode.StartTagAndEndTag;
        output.TagName = "form";
        output.AddClass("container", HtmlEncoder.Default);

        if (Method == HttpMethod.Get || Method == HttpMethod.Post) {
            // GET and POST can use standard method attribute
            output.Attributes.SetAttribute("method", Method.ToString());
        } else {
            // other methods have to use custom attribute
            output.Attributes.SetAttribute(PJax.MethodAttribute, Method.ToString());
        }

        if (!string.IsNullOrWhiteSpace(Controller)) {
            Controller = Controller.StripController();
        }
        output.Attributes.SetAttribute("id", $"{Action}{Controller}Form");

        var urlHelper = _UrlHelperFactory.GetUrlHelper(HtmlHelper!.ViewContext);
        output.Attributes.SetAttribute("action", urlHelper.Action(Action, Controller, RouteValues));

        hiddenInputs.ForEach(x => output.Content.AppendHtml(x));

        output.Content.AppendHtml(HtmlHelper.AntiForgeryToken());
        output.Content.AppendHtml(output.GetChildContentAsync().Result);

        await base.ProcessAsync(context, output);
    }
}
