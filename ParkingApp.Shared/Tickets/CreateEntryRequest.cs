using ParkingApp.Shared.Spots;

namespace ParkingApp.Shared.Tickets;

/// <summary>
/// The employee's request to register a vehicle entry in the active branch.
/// The branch is not part of the request: the server takes it from the token.
/// </summary>
public class CreateEntryRequest
{
	/// <summary>The plate as typed. The server normalizes and checks it.</summary>
	public string LicensePlate { get; set; } = string.Empty;

	/// <summary>The kind of the plate (Greek or foreign).</summary>
	public PlateKind PlateKind { get; set; }

	/// <summary>
	/// The size of the vehicle. It is used only to check that the spot is large enough;
	/// it is not stored, because charging depends on the spot size.
	/// </summary>
	public SpotSize VehicleSize { get; set; }

	/// <summary>The spot the employee confirmed (the suggested one or another one).</summary>
	public Guid SpotId { get; set; }
}
