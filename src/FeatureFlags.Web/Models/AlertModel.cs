using FeatureFlags.Constants;

namespace FeatureFlags.Models;

public sealed record AlertModel {
    public string Content { get; init; } = "";
    public AlertType AlertType { get; init; } = AlertType.Success;
    public bool CanDismiss { get; init; } = true;
    public Icon? Icon { get; init; }
}
