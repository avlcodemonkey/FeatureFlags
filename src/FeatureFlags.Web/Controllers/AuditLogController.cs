using FeatureFlags.Attributes;
using FeatureFlags.Extensions;
using FeatureFlags.Models;
using FeatureFlags.Resources;
using FeatureFlags.Services;
using Microsoft.AspNetCore.Mvc;

namespace FeatureFlags.Controllers;

/// <summary>
/// Provides actions for managing and viewing audit logs.
/// </summary>
public class AuditLogController(IAuditLogService auditLogService, IUserService userService, ILogger<AuditLogController> logger) : BaseController(logger) {
    private readonly IUserService _UserService = userService;
    private readonly IAuditLogService _AuditLogService = auditLogService;

    private const string _IndexView = "Index";
    private const string _ViewView = "View";

    /// <summary>
    /// Displays the audit log search view with the specified search criteria.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> that renders the audit log search view.</returns>
    [HttpGet]
    public IActionResult Index(AuditLogSearchModel model) => View(_IndexView, model);

    /// <summary>
    /// Searches audit logs based on the specified criteria.
    /// </summary>
    /// <remarks>This method is intended to be used in AJAX requests and will only respond to such requests.</remarks>
    /// <returns>An <see cref="IActionResult"/> containing the search results.</returns>
    [HttpPost, ParentAction(nameof(Index)), AjaxRequestOnly]
    public async Task<IActionResult> Search(AuditLogSearchModel model, CancellationToken cancellationToken = default)
        => Ok(await _AuditLogService.SearchLogsAsync(model, cancellationToken));

    /// <summary>
    /// Retrieves a list of users whose names match the specified query string.
    /// </summary>
    /// <remarks>This method is intended to be used in AJAX requests and will only respond to such requests.</remarks>
    /// <returns>An <see cref="IActionResult"/> with a collection of users matching the query string. Returns empty collection if no matches found.</returns>
    [HttpGet, ParentAction(nameof(Index)), AjaxRequestOnly]
    public async Task<IActionResult> UserList(string query, CancellationToken cancellationToken = default)
        => Ok(await _UserService.FindAutocompleteUsersByNameAsync(query, cancellationToken));

    /// <summary>
    /// Retrieves and displays the audit log entry with the specified ID.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> that renders the audit log entry if found, else redirects to the index.</returns>
    [HttpGet]
    public async Task<IActionResult> View(long id, CancellationToken cancellationToken = default) {
        var log = await _AuditLogService.GetLogByIdAsync(id, cancellationToken);
        if (log == null) {
            ViewData.AddError(Core.ErrorInvalidId);
        }

        return log == null ? Index(new AuditLogSearchModel()) : View(_ViewView, log);
    }
}
