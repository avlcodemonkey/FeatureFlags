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

/// <summary>
/// Renders a form element with support for model binding, HTTP method selection, and hidden fields for versioned and audited models.
/// </summary>
/// <param name="htmlHelper">HtmlHelper for rendering.</param>
/// <param name="urlHelperFactory">Factory for creating URL helpers.</param>
public sealed class FormContentTagHelper(IHtmlHelper htmlHelper, IUrlHelperFactory urlHelperFactory) : BaseTagHelper(htmlHelper) {
    private readonly IUrlHelperFactory _UrlHelperFactory = urlHelperFactory;

    /// <summary>
    /// Gets or sets the action name for the form submission.
    /// </summary>
    public string? Action { get; set; }

    /// <summary>
    /// Gets or sets the controller name for the form submission.
    /// </summary>
    public string? Controller { get; set; }

    /// <summary>
    /// Gets or sets the model object for the form.
    /// </summary>
    public object? For { get; set; }

    /// <summary>
    /// Gets or sets the HTTP method for the form submission.
    /// </summary>
    public HttpMethod Method { get; set; } = HttpMethod.Post;

    /// <summary>
    /// Gets or sets the route values for the form action.
    /// </summary>
    public object? RouteValues { get; set; }

    /// <summary>
    /// Processes the tag helper and generates the form output.
    /// </summary>
    /// <param name="context">Context for tag helper execution.</param>
    /// <param name="output">Output for tag helper content.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
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
