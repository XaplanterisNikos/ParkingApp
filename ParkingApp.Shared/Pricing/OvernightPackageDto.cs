using ParkingApp.Shared.Spots;

namespace ParkingApp.Shared.Pricing;

/// <summary>An overnight package of a price list.</summary>
public class OvernightPackageDto
{
	/// <summary>The package id. Ignored when a price list is saved: every version gets new packages.</summary>
	public Guid Id { get; set; }

	/// <summary>The name the owner gives to the package.</summary>
	public string Name { get; set; } = string.Empty;

	/// <summary>The spot size the package is for.</summary>
	public SpotSize Size { get; set; } = SpotSize.Car;

	/// <summary>How many nights the package covers.</summary>
	public int Nights { get; set; } = 1;

	/// <summary>The price of the package, VAT included.</summary>
	public decimal Price { get; set; }
}
