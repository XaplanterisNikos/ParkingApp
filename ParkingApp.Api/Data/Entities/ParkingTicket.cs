using ParkingApp.Shared.Tickets;

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

	/// <summary>
	/// The kind of the plate. A Greek plate is stored with Latin letters, so the kind is
	/// needed to show it again with Greek letters (for example on the receipt).
	/// </summary>
	public PlateKind PlateKind { get; set; }

	/// <summary>When the vehicle entered (UTC-based, with offset).</summary>
	public DateTimeOffset EnteredAt { get; set; }

	/// <summary>The employee who registered the entry (ApplicationUser.Id).</summary>
	public required string EnteredByEmployeeId { get; set; }

	/// <summary>When the vehicle exited; <c>null</c> while the ticket is active.</summary>
	public DateTimeOffset? ExitedAt { get; set; }

	/// <summary>The employee who registered the exit; <c>null</c> while the ticket is active.</summary>
	public string? ExitedByEmployeeId { get; set; }

	/// <summary>
	/// The price list version that was current when the vehicle entered. At exit the employee
	/// may charge with this version or with the current one. <c>null</c> for tickets issued
	/// before price lists existed.
	/// </summary>
	public Guid? TariffVersionId { get; set; }

	/// <summary>The overnight package chosen at entry; <c>null</c> when the normal price list applies.</summary>
	public Guid? OvernightPackageId { get; set; }

	/// <summary>The amount charged at exit, VAT included; <c>null</c> while the ticket is active.</summary>
	public decimal? AmountCharged { get; set; }

	/// <summary>
	/// The VAT rate (percent) of the charge, kept with the ticket so the invoice stays correct
	/// even if the owner changes the rate later; <c>null</c> while the ticket is active.
	/// </summary>
	public decimal? VatRate { get; set; }

	// --- fields to add in later slices ---
	// ShiftId (nullable) — when shifts exist
	// Status + void reason — ticket voiding instead of deletion
}