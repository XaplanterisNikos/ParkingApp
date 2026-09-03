namespace ParkingApp.Api.Data.Entities;

/// <summary>
/// A floor (level) within a parking branch. The owner gives it a name
/// (e.g. "Υπόγειο", "Ισόγειο"), and it holds the parking spots on that level.
/// </summary>
public class Floor : TenantEntity
{
	/// <summary>The branch this floor belongs to.</summary>
	public Guid BranchId { get; set; }
	/// <summary>The floor's display name, defined by the owner.</summary>
	public required string Name { get; set;  }

	// Fields to add in later slices, when their screens exist ---
	// Opening hours (a floor may open/close independently)
	// Capacity / total spot count
	// Access restrictions (height limit, EV-only, etc.)
	// Per-floor pricing overrides
}
