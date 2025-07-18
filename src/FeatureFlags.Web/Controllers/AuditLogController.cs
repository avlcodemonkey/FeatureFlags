using FeatureFlags.Attributes;
using FeatureFlags.Extensions;
using FeatureFlags.Models;
using FeatureFlags.Resources;
using FeatureFlags.Services;
using Microsoft.AspNetCore.Mvc;

namespace FeatureFlags.Controllers;

public class AuditLogController(IAuditLogService auditLogService, IUserService userService, ILogger<AuditLogController> logger) : BaseController(logger) {
    private readonly IUserService _UserService = userService;
    private readonly IAuditLogService _AuditLogService = auditLogService;

    private const string _IndexView = "Index";
    private const string _ViewView = "View";

    [HttpGet]
    public IActionResult Index(AuditLogSearchModel model) => View(_IndexView, model);

    [HttpPost, ParentAction(nameof(Index)), AjaxRequestOnly]
    public async Task<IActionResult> Search(AuditLogSearchModel model, CancellationToken cancellationToken = default)
        => Ok(await _AuditLogService.SearchLogsAsync(model, cancellationToken));

    [HttpGet, ParentAction(nameof(Index)), AjaxRequestOnly]
    public async Task<IActionResult> UserList(string query, CancellationToken cancellationToken = default)
        => Ok(await _UserService.FindAutocompleteUsersByNameAsync(query, cancellationToken));

    [HttpGet]
    public async Task<IActionResult> View(long id, CancellationToken cancellationToken = default) {
        var log = await _AuditLogService.GetLogByIdAsync(id, cancellationToken);
        if (log == null) {
            ViewData.AddError(Core.ErrorInvalidId);
        }

        return log == null ? Index(new AuditLogSearchModel()) : View(_ViewView, log);
    }
}
