using FeatureFlags.Extensions;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;

namespace FeatureFlags.Attributes;

/// <summary>
/// Specifies that an action should accept ajax requests only.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class AjaxRequestOnlyAttribute : ActionMethodSelectorAttribute {
    public override bool IsValidForRequest(RouteContext routeContext, ActionDescriptor action) => routeContext.HttpContext.Request.IsAjaxRequest();
}
