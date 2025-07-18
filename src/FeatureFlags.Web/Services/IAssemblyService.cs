namespace FeatureFlags.Services;

/// <summary>
/// Provides access to `Assembly` functions for testing.
/// </summary>
public interface IAssemblyService {
    /// <summary>
    /// Creates a dictionary of all controller actions in the assembly that require authorization.
    /// </summary>
    /// <returns>Dictionary of actions with lowercase keys.</returns>
    Dictionary<string, string> GetActionList();
}
