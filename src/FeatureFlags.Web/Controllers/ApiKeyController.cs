using FeatureFlags.Attributes;
using FeatureFlags.Extensions;
using FeatureFlags.Models;
using FeatureFlags.Resources;
using FeatureFlags.Services;
using FeatureFlags.Utils;
using Microsoft.AspNetCore.Mvc;

namespace FeatureFlags.Controllers;

/// <summary>
/// Provides actions for managing API keys, including listing, creating, and deleting keys.
/// </summary>
/// <param name="apiKeyService">Service for API key data.</param>
/// <param name="logger">ILogger for logging.</param>
public class ApiKeyController(IApiKeyService apiKeyService, ILogger<ApiKeyController> logger) : BaseController(logger) {
    private readonly IApiKeyService _ApiKeyService = apiKeyService;

    private const string _IndexView = "Index";
    private const string _CreateView = "Create";

    /// <summary>
    /// Renders the API keys landing page with the table of API keys.
    /// </summary>
    [HttpGet]
    public IActionResult Index() => View(_IndexView);

    /// <summary>
    /// Returns the API key list as json.
    /// </summary>
    [HttpGet, ParentAction(nameof(Index)), AjaxRequestOnly]
    public async Task<IActionResult> List(CancellationToken cancellationToken = default)
        => Ok((await _ApiKeyService.GetAllApiKeysAsync(cancellationToken)).Select(x =>
            new ApiKeyListResultModel { Id = x.Id, Name = x.Name, Key = x.Key, CreatedDate = x.CreatedDate }
        ));

    /// <summary>
    /// Renders the form to create an API key.
    /// </summary>
    [HttpGet]
    public IActionResult Create() => View(_CreateView, new ApiKeyModel { Key = KeyGenerator.GetApiKey() });

    /// <summary>
    /// Saves new API key if valid. Renders the create page on error, or redirects to index page.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ApiKeyModel model, CancellationToken cancellationToken = default) {
        if (!ModelState.IsValid) {
            return ViewWithError(_CreateView, model, ModelState);
        }

        (var success, var message) = await _ApiKeyService.SaveApiKeyAsync(model, cancellationToken);
        if (!success) {
            return ViewWithError(_CreateView, model, message);
        }

        ViewData.AddMessage(message);
        return IndexWithPushState();
    }

    /// <summary>
    /// Deletes the API key if valid, and renders the index page.
    /// </summary>
    [HttpDelete]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default) {
        if (!await _ApiKeyService.DeleteApiKeyAsync(id, cancellationToken)) {
            return ViewWithError(_IndexView, null, ApiKeys.ErrorDeletingApiKey);
        }

        ViewData.AddMessage(ApiKeys.SuccessDeletingApiKey);
        return IndexWithPushState();
    }

    /// <summary>
    /// Helper method to add a header to pushState to Index and return the Index action.
    /// </summary>
    private IActionResult IndexWithPushState() {
        AddPushState(nameof(Index));
        return Index();
    }
}
