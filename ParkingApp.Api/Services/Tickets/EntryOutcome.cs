namespace ParkingApp.Api.Services.Tickets;

/// <summary>
/// The possible results of a vehicle entry. The controller maps each one to an HTTP status.
/// </summary>
public enum EntryOutcome
{
	/// <summary>The ticket was issued (200).</summary>
	Created,

	/// <summary>The request breaks a rule: bad plate, unknown size, or a spot that is too small (400).</summary>
	Invalid,

	/// <summary>The spot does not exist in the active branch (404).</summary>
	SpotNotFound,

	/// <summary>Another desk took the spot first (409).</summary>
	SpotTaken,

	/// <summary>The same plate already has an active ticket in this branch (409).</summary>
	PlateAlreadyInside
}
