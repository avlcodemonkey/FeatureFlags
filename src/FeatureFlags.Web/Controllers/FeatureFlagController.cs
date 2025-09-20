using FeatureFlags.Attributes;
using FeatureFlags.Extensions;
using FeatureFlags.Models;
using FeatureFlags.Resources;
using FeatureFlags.Services;
using FeatureFlags.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.FeatureManagement;

namespace FeatureFlags.Controllers;

/// <summary>
/// Provides functionality for managing feature flags, including rendering views, enabling or disabling flags,
/// and clearing the feature flag cache.
/// </summary>
public class FeatureFlagController(IFeatureFlagService featureFlagService, FeatureManager featureManager, ILogger<FeatureFlagController> logger)
    : BaseController(logger) {

    private readonly IFeatureFlagService _FeatureFlagService = featureFlagService;
    private readonly FeatureManager _FeatureManager = featureManager;

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
    public async Task<IActionResult> List(CancellationToken cancellationToken = default) {
        var flags = await _FeatureFlagService.GetAllFeatureFlagsAsync(cancellationToken);
        if (flags == null || !flags.Any()) {
            return Ok(new List<FeatureFlagListResultModel>());
        }

        // create a featureManager with our internal provider to get the status of each flag
        var dynamicFeatureManager = CreateFeatureManager();

        var flagList = new List<FeatureFlagListResultModel>();
        foreach (var flag in flags) {
            var evaluationText = "";
            // we want to catch any errors here so one bad flag config doesn't break the whole list
            try {
                if (await dynamicFeatureManager.IsEnabledAsync(flag.Name, cancellationToken)) {
                    evaluationText = Flags.Enabled;
                } else {
                    evaluationText = Flags.Disabled;
                }
            } catch (Exception) {
                evaluationText = Flags.Error;
            }
            flagList.Add(new FeatureFlagListResultModel { Id = flag.Id, Name = flag.Name, Status = flag.Status, EvaluationText = evaluationText });
        }

        return Ok(flagList);
    }

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

        // create a featureManager with our internal provider to check the status of the flag
        var dynamicFeatureManager = CreateFeatureManager();
        var evaluationText = "";
        try {
            await dynamicFeatureManager.IsEnabledAsync(model.Name, cancellationToken);
        } catch (Exception ex) {
            evaluationText = $"{Flags.ErrorEvaluatingFlag} {ex.Message}";
        }
        if (!string.IsNullOrEmpty(evaluationText)) {
            ViewData.AddError(evaluationText);
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
    /// Helper method to add a header to pushState to Index and return the Index action.
    /// </summary>
    private IActionResult IndexWithPushState() {
        AddPushState(nameof(Index));
        return Index();
    }

    /// <summary>
    /// Creates and initializes a new instance of the <see cref="FeatureManager"/> class.
    /// Existing <see cref="FeatureManager"/> instance's feature filters will be used to configure the new instance.
    /// </summary>
    /// <returns>A new <see cref="FeatureManager"/> instance configured with the available feature filters.</returns>
    private FeatureManager CreateFeatureManager() => new(new InternalFeatureDefinitionProvider(_FeatureFlagService)) {
        FeatureFilters = _FeatureManager.FeatureFilters
    };
}
