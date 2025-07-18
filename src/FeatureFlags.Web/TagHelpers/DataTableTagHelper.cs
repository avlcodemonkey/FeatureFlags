using FeatureFlags.Models;
using FeatureFlags.Resources;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FeatureFlags.TagHelpers;

public sealed class DataTableTagHelper(IHtmlHelper htmlHelper) : BaseTagHelper(htmlHelper) {
    public string? Key { get; set; }

    public string? SrcUrl { get; set; }

    public string? SrcForm { get; set; }

    public int MaxResults { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output) {
        Contextualize();

        if (string.IsNullOrWhiteSpace(Key) || (string.IsNullOrWhiteSpace(SrcUrl) && string.IsNullOrWhiteSpace(SrcForm))) {
            output.SuppressOutput();
            await base.ProcessAsync(context, output);
            return;
        }

        var table = new TagBuilder("table");
        table.AddCssClass("col");
        table.AddCssClass("striped");
        table.InnerHtml.AppendHtml(await output.GetChildContentAsync());

        var sortAscSpan = new TagBuilder("span");
        sortAscSpan.AddCssClass("sort-icon");
        sortAscSpan.MergeAttribute("data-table-sort-asc", "");
        sortAscSpan.MergeAttribute("aria-label", Core.Ascending);
        sortAscSpan.InnerHtml.AppendHtml(await HtmlHelper!.PartialAsync("Icons/_CaretUp"));
        var sortAscTemplate = new TagBuilder("template");
        sortAscTemplate.MergeAttribute("data-table-sort-asc-template", "");
        sortAscTemplate.InnerHtml.AppendHtml(sortAscSpan);

        var sortDescSpan = new TagBuilder("span");
        sortDescSpan.AddCssClass("sort-icon");
        sortDescSpan.MergeAttribute("data-table-sort-desc", "");
        sortDescSpan.MergeAttribute("aria-label", Core.Descending);
        sortDescSpan.InnerHtml.AppendHtml(await HtmlHelper!.PartialAsync("Icons/_CaretDown"));
        var sortDescTemplate = new TagBuilder("template");
        sortDescTemplate.MergeAttribute("data-table-sort-desc-template", "");
        sortDescTemplate.InnerHtml.AppendHtml(sortDescSpan);

        var rowDiv = new TagBuilder("div");
        rowDiv.AddCssClass("row");
        rowDiv.AddCssClass("h-scroll");
        rowDiv.InnerHtml.AppendHtml(sortAscTemplate);
        rowDiv.InnerHtml.AppendHtml(sortDescTemplate);
        rowDiv.InnerHtml.AppendHtml(table);

        var containerDiv = new TagBuilder("div");
        containerDiv.AddCssClass("container");
        containerDiv.InnerHtml.AppendHtml(await HtmlHelper!.PartialAsync("_DataTableHeader", new DataTableModel { HideSearch = !string.IsNullOrEmpty(SrcForm) }));
        containerDiv.InnerHtml.AppendHtml(rowDiv);
        containerDiv.InnerHtml.AppendHtml(await HtmlHelper!.PartialAsync("_DataTableFooter"));

        output.TagName = "nilla-table";
        output.Attributes.SetAttribute("data-key", Key);
        output.Attributes.SetAttribute("data-src-url", SrcUrl);
        output.Attributes.SetAttribute("data-src-form", SrcForm);
        output.Attributes.SetAttribute("data-max-results", MaxResults);
        output.Content.AppendHtml(containerDiv);

        await base.ProcessAsync(context, output);
    }
}
