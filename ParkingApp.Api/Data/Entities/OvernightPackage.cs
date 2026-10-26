using ParkingApp.Shared.Spots;

namespace ParkingApp.Api.Data.Entities;

/// <summary>
/// An overnight package the owner offers (e.g. "Σαββατοκύριακο": 2 nights for a car).
/// It is chosen at entry and replaces the normal price list for its period.
/// </summary>
public class OvernightPackage : TenantEntity
{
	/// <summary>The price list version this package belongs to.</summary>
	public Guid TariffVersionId { get; set; }

	/// <summary>The name the owner gives to the package, shown to the employee.</summary>
	public required string Name { get; set; }

	/// <summary>The spot size the package is for.</summary>
	public SpotSize Size { get; set; }

	/// <summary>
	/// How many nights the package covers. It ends on the date of entry plus this number
	/// of days, at the version's checkout time.
	/// </summary>
	public int Nights { get; set; }

	/// <summary>The price of the package, VAT included.</summary>
	public decimal Price { get; set; }
}
