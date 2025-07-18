namespace FeatureFlags.Models;

/// <summary>
/// Used for address autocomplete.
/// </summary>
public sealed record AddressSearchResult {
    public string Name { get; init; } = "";

    public string Address1 { get; init; } = "";

    public string City { get; init; } = "";

    public string StateCode { get; init; } = "";

    public string PostalCode { get; init; } = "";

    public string CountryCode { get; init; } = "";

    public double Longitude { get; set; }

    public double Latitude { get; set; }
}
