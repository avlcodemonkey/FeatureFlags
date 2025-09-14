using FeatureFlags.Attributes;
using FeatureFlags.Client;
using FeatureFlags.Extensions;
using FeatureFlags.Models;
using FeatureFlags.Resources;
using FeatureFlags.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FeatureFlags.Controllers;

/// <summary>
/// Provides functionality for managing feature flags, including rendering views, enabling or disabling flags,
/// and clearing the feature flag cache.
/// </summary>
public class FeatureFlagController(IFeatureFlagService featureFlagService, IFeatureFlagClient featureFlagClient, ILogger<FeatureFlagController> logger)
    : BaseController(logger) {

    private readonly IFeatureFlagService _FeatureFlagService = featureFlagService;
    private readonly IFeatureFlagClient _FeatureFlagClient = featureFlagClient;

    private const string _IndexView = "Index";
    private const string _CreateEditView = "CreateEdit";

    /// <summary>
    /// Renders the feature flag landing page with the table of flags.
    /// </summary>
    [HttpGet]
    public IActionResult Index() => View(_IndexView);

    /// <summary>
    /// Returns the feature flag list as json.
    /// </summary>
    [HttpGet, ParentAction(nameof(Index)), AjaxRequestOnly]
    public async Task<IActionResult> List(CancellationToken cancellationToken = default)
        => Ok((await _FeatureFlagService.GetAllFeatureFlagsAsync(cancellationToken))
            .Select(x => new FeatureFlagListResultModel { Id = x.Id, Name = x.Name, Status = x.Status }));

    /// <summary>
    /// Renders the form to create a feature flag.
    /// </summary>
    [HttpGet]
    public IActionResult Create() => View(_CreateEditView, new FeatureFlagModel());

    /// <summary>
    /// Saves new feature flage if valid. Renders the create page on error, or redirects to index page.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(FeatureFlagModel model, CancellationToken cancellationToken = default) => await Save(model, cancellationToken);

    /// <summary>
    /// Renders the form to edit a feature flag, or index page on error.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken = default) {
        var model = await _FeatureFlagService.GetFeatureFlagByIdAsync(id, cancellationToken);
        if (model == null) {
            return ViewWithError(_IndexView, null, Core.ErrorInvalidId);
        }
        return View(_CreateEditView, model);
    }

    /// <summary>
    /// Saves updated feature falg if valid. Renders the edit page on error, or redirects to index page.
    /// </summary>
    [HttpPut, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(FeatureFlagModel model, CancellationToken cancellationToken = default) => await Save(model, cancellationToken);

    /// <summary>
    /// Save the model using the feature flag service.  Used by Create and Edit.
    /// </summary>
    private async Task<IActionResult> Save(FeatureFlagModel model, CancellationToken cancellationToken = default) {
        if (!ModelState.IsValid) {
            var filterErrors = new List<ModelError>();
            var modelErrors = new List<ModelError>();

            foreach (var entry in ModelState) {
                var errors = entry.Value.Errors;
                if (errors.Count == 0) {
                    continue;
                }

                if (entry.Key.StartsWith(nameof(FeatureFlagModel.Filters))) {
                    filterErrors.AddRange(errors);
                } else {
                    modelErrors.AddRange(errors);
                }
            }

            var errorMessages = new List<string>();
            if (modelErrors.Count != 0) {
                errorMessages.AddRange(modelErrors.Select(x => x.ErrorMessage));
            }
            if (filterErrors.Count != 0) {
                errorMessages.Add(Flags.ErrorCheckFilters);
            }
            ViewData.AddError(string.Join(" <br />", errorMessages));

            return View(_CreateEditView, model);
        }

        (var success, var message) = await _FeatureFlagService.SaveFeatureFlagAsync(model, cancellationToken);
        if (!success) {
            return ViewWithError(_CreateEditView, model, message);
        }

        ViewData.AddMessage(message);
        return IndexWithPushState();
    }

    /// <summary>
    /// Deletes the feature flag if valid, and renders the index page.
    /// </summary>
    [HttpDelete]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default) {
        if (!await _FeatureFlagService.DeleteFeatureFlagAsync(id, cancellationToken)) {
            return ViewWithError(_IndexView, null, Flags.ErrorDeletingFlag);
        }

        ViewData.AddMessage(Flags.SuccessDeletingFlag);
        return IndexWithPushState();
    }

    /// <summary>
    /// Clears the feature flag cache. Renders the index page.
    /// </summary>
    [HttpGet]
    public IActionResult ClearCache() {
        _FeatureFlagClient.ClearCache();
        ViewData.AddMessage(Flags.SuccessClearingCache);
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
