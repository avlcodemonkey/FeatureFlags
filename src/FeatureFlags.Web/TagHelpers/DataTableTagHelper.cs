using FeatureFlags.Models;
using FeatureFlags.Resources;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FeatureFlags.TagHelpers;

/// <summary>
/// Renders a data table with sorting, search, and customizable data source options.
/// </summary>
/// <param name="htmlHelper">HtmlHelper for rendering.</param>
public sealed class DataTableTagHelper(IHtmlHelper htmlHelper) : BaseTagHelper(htmlHelper) {
    /// <summary>
    /// Gets or sets the unique key for the data table instance.
    /// </summary>
    public string? Key { get; set; }

    /// <summary>
    /// Gets or sets the URL to fetch data for the table.
    /// </summary>
    public string? SrcUrl { get; set; }

    /// <summary>
    /// Gets or sets the form selector or name to use as a data source.
    /// </summary>
    public string? SrcForm { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of results to display.
    /// </summary>
    public int MaxResults { get; set; }

    /// <summary>
    /// Processes the tag helper and generates the data table output.
    /// </summary>
    /// <param name="context">Context for tag helper execution.</param>
    /// <param name="output">Output for tag helper content.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
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
