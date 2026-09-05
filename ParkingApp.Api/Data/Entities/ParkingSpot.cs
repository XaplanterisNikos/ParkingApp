using ParkingApp.Shared.Spots;

namespace ParkingApp.Api.Data.Entities;

/// <summary>
/// A single parking spot on a floor. Its occupancy is NOT stored here — it is derived
/// from movements (Feature Slice 3). This entity is just the catalogue of spots.
/// </summary>
public class ParkingSpot : TenantEntity
{
	/// <summary>The floor this spot belongs to.</summary>
	public Guid FloorId { get; set; }
	/// <summary>The spot's code/number (e.g. "15", "A3").</summary>
	public required string Number { get; set; }
	/// <summary>The spot's size category (which vehicles fit).</summary>
	public SpotSize Size { get; set; }
}
