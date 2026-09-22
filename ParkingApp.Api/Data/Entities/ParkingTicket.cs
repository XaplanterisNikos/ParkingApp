namespace ParkingApp.Api.Data.Entities;

/// <summary>
/// A parking ticket: one vehicle's stay in one spot, from entry to exit.
/// A ticket with no <see cref="ExitedAt"/> is ACTIVE — its spot is occupied.
/// Spot occupancy is derived from active tickets; it is not stored anywhere else.
/// </summary>
public class ParkingTicket : TenantEntity
{
	/// <summary>
	/// The branch the ticket was issued in. Denormalized (also reachable via spot → floor)
	/// so branch-scoped queries don't need a join.
	/// </summary>
	public Guid BranchId { get; set; }

	/// <summary>The spot the vehicle was assigned to.</summary>
	public Guid ParkingSpotId { get; set; }

	/// <summary>The vehicle's licence plate, stored normalized (uppercase, Latin, no spaces/dashes).</summary>
	public required string LicensePlate { get; set; }

	/// <summary>When the vehicle entered (UTC-based, with offset).</summary>
	public DateTimeOffset EnteredAt { get; set; }

	/// <summary>The employee who registered the entry (ApplicationUser.Id).</summary>
	public required string EnteredByEmployeeId { get; set; }

	/// <summary>When the vehicle exited; <c>null</c> while the ticket is active.</summary>
	public DateTimeOffset? ExitedAt { get; set; }

	/// <summary>The employee who registered the exit; <c>null</c> while the ticket is active.</summary>
	public string? ExitedByEmployeeId { get; set; }

	// --- fields to add in later slices ---
	// ShiftId (nullable) — when shifts exist
	// Amount charged — when owner pricing exists
	// Status + void reason — ticket voiding instead of deletion
}