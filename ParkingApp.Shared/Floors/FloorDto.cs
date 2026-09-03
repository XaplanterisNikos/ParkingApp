namespace ParkingApp.Shared.Floors;

/// <summary>
/// A floor as exposed to the client. No CompanyId — tenant scoping is server-side.
/// </summary>
public class FloorDto
{
	/// <summary>The floor's unique identifier.</summary>
	public Guid Id { get; set; }
	/// <summary>The floor's display name.</summary>
	public required string Name { get; set; }
}
