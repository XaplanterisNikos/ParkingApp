namespace ParkingApp.Api.Data.Entities;

/// <summary>
/// A parking branch (a physical parking location) that belongs to a company.
/// A company can have zero, one, or many branches.
/// </summary>
public class Branch : TenantEntity
{
	/// <summary>The branch's display name (e.g. "Parking Κέντρο").</summary>
	public required string Name { get; set; }

	// --- fields to add , when their screens exist ---
	// Capacity / total spots
	// Amenities (car wash, cleaning, ...)
	// Opening hours / 24h flag
	// Holidays schedule
	// Security / guarding
	// Special offer packages / pricing
	// Address / location (map coordinates)
	// Contact info
}
