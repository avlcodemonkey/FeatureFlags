using FeatureFlags.Extensions;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;

namespace FeatureFlags.Attributes;

/// <summary>
/// Specifies that an action should accept ajax requests only.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class AjaxRequestOnlyAttribute : ActionMethodSelectorAttribute {
    /// <summary>
    /// Determines whether the current request is an AJAX request.
    /// </summary>
    /// <remarks>An AJAX request is typically identified by the presence of the "X-Requested-With" header set to "XMLHttpRequest".</remarks>
    /// <param name="routeContext">Context of the route being processed. This provides access to the HTTP context and other routing information.</param>
    /// <param name="action">Action descriptor representing the action being invoked. This parameter is not used in the validation.</param>
    /// <returns><see langword="true"/> if the request is an AJAX request; otherwise, <see langword="false"/>.</returns>
    public override bool IsValidForRequest(RouteContext routeContext, ActionDescriptor action) => routeContext.HttpContext.Request.IsAjaxRequest();
}
