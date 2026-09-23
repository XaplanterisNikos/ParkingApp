using ParkingApp.Shared.Spots;

namespace ParkingApp.Shared.Console;

/// <summary>
/// One spot on the employee console's spot map: what it is and whether it is taken right now.
/// Occupancy is derived on the server from active tickets; it is not stored on the spot.
/// </summary>
public class SpotStatusDto
{
	/// <summary>The spot's unique identifier — sent back when the employee confirms an entry.</summary>
	public Guid Id { get; set; }

	/// <summary>The spot's code (e.g. "AC12") — shown in the box and printed on the ticket.</summary>
	public required string Number { get; set; }

	/// <summary>The spot's size category; the UI uses it to allow only spots that fit the vehicle.</summary>
	public SpotSize Size { get; set; }

	/// <summary>True when the spot has an active ticket (red box); false when it is free (green box).</summary>
	public bool IsOccupied { get; set; }
}