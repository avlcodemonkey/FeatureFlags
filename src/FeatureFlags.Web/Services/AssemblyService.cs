using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using FeatureFlags.Attributes;
using FeatureFlags.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeatureFlags.Services;

/// <inheritdoc />
[SuppressMessage("Minor Code Smell", "S6608:Prefer indexing instead of \"Enumerable\" methods on types implementing \"IList\"",
    Justification = "Using Last() in a lambda is simpler.")]
public sealed class AssemblyService() : IAssemblyService {
    /// <inheritdoc />
    public Dictionary<string, string> GetActionList() => Assembly.GetExecutingAssembly().GetTypes()
        .Where(x => typeof(Controller).IsAssignableFrom(x)) // filter to controllers only
        .SelectMany(x => x.GetMethods())
        .Where(x => x.IsPublic // public methods only
            && !x.IsDefined(typeof(NonActionAttribute))  // action methods only
            && (x.IsDefined(typeof(AuthorizeAttribute)) || x.DeclaringType?.IsDefined(typeof(AuthorizeAttribute)) == true) // requires authorization or parent type does
            && !(x.IsDefined(typeof(AllowAnonymousAttribute)) || x.DeclaringType?.IsDefined(typeof(AllowAnonymousAttribute)) == true) // doesn't include allow anonymous
            && !x.IsDefined(typeof(ParentActionAttribute))) // doesn't include methods with ParentAction
        .Select(x => $"{x.DeclaringType?.FullName?.Split('.').Last().StripController()}.{x.Name}")
        .Distinct()
        .ToDictionary(x => x.ToLower(CultureInfo.InvariantCulture), x => x);
}
