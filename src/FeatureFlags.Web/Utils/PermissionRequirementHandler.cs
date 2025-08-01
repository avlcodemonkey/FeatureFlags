using System.Globalization;
using System.Reflection;
using FeatureFlags.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace FeatureFlags.Utils;

/// <summary>
/// Handles authorization for permission-based requirements using controller action metadata.
/// </summary>
public sealed class PermissionRequirementHandler : AuthorizationHandler<PermissionRequirement> {
    /// <summary>
    /// Policy name for permission-based authorization.
    /// </summary>
    public const string PolicyName = "HasPermission";

    /// <summary>
    /// Evaluates whether the current user meets the specified permission requirement.
    /// </summary>
    /// <param name="context">Authorization context containing user and resource information.</param>
    /// <param name="requirement">Permission requirement to evaluate.</param>
    /// <returns>A completed task when evaluation is finished.</returns>
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement) {
        Endpoint? endpoint = null;
        if (context.Resource is HttpContext httpContext) {
            endpoint = httpContext.GetEndpoint();
        } else if (context.Resource is Endpoint endpoint2) {
            endpoint = endpoint2;
        }
        if (endpoint == null) {
            return Task.CompletedTask;
        }

        var descriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
        if (descriptor == null) {
            return Task.CompletedTask;
        }

        var parentActions = descriptor.MethodInfo.GetCustomAttributes<ParentActionAttribute>().Where(x => !string.IsNullOrWhiteSpace(x.Action));
        // if an action has a ParentActionAttribute then use the parent action values, else use the action value
        var matchingActions = parentActions.Any() ? parentActions.SelectMany(x => x.Action.Split(',')).Where(x => !string.IsNullOrWhiteSpace(x))
            : new List<string>() { descriptor.ActionName };

        if (matchingActions.Select(x => $"{descriptor?.ControllerName}.{x}".ToLower(CultureInfo.InvariantCulture)).Any(context.User.IsInRole)) {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Represents a permission requirement for authorization.
/// </summary>
public sealed class PermissionRequirement : IAuthorizationRequirement {
    /// <summary>
    /// Initializes a new instance of the <see cref="PermissionRequirement"/> class.
    /// </summary>
    public PermissionRequirement() { }
}
