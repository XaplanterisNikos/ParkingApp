namespace ParkingApp.Api.Pricing;

/// <summary>What the calculator needs to know about an overnight package.</summary>
/// <param name="Nights">How many nights the package covers (at least 1).</param>
/// <param name="Price">The price of the package, VAT included.</param>
public sealed record PackageSnapshot(int Nights, decimal Price);
