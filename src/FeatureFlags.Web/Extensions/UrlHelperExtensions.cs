using FeatureFlags.Models;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace FeatureFlags.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="IUrlHelper"/> interface to assist with URL generation.
/// </summary>
public static class UrlHelperExtensions {
    /// <summary>
    /// Builds a URL for an action removing Id from route data and appending the id placeholder for mustache to use.
    /// </summary>
    public static string ActionForMustache(this IUrlHelper urlHelper, string actionName) {
        var id = nameof(IAuditedModel.Id).ToLower();
        var routeParams = new Dictionary<string, string> { { id, string.Empty } };
        return $"{urlHelper.Action(actionName, routeParams)}/{{{{{id}}}}}";
    }

    /// <summary>
    /// Converts a relative URL to an absolute one.
    /// </summary>
    public static string ConvertToAbsolute(this IUrlHelper urlHelper, string relativeUrl) {
        if (relativeUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || relativeUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) {
            return relativeUrl;
        }
        if (relativeUrl.StartsWith("~", StringComparison.OrdinalIgnoreCase)) {
            relativeUrl = relativeUrl[1..];
        }

        var requestUri = new Uri(urlHelper.ActionContext.HttpContext.Request.GetEncodedUrl());
        var uri = new Uri(requestUri, relativeUrl);
        return uri.AbsoluteUri;
    }
}
