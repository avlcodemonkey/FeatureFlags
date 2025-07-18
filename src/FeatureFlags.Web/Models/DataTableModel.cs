namespace FeatureFlags.Models;

public sealed record DataTableModel {
    public bool HideSearch { get; init; } = false;
}
