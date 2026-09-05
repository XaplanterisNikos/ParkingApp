namespace ParkingApp.Shared.Spots;

/// <summary>
/// A parking spot as exposed to the client.
/// </summary>
public class SpotDto
{
	/// <summary>The spot's unique identifier.</summary>
	public Guid Id { get; set; }

	/// <summary>The spot's code/number (e.g. "15", "A3").</summary>
	public required string Number { get; set; }

	/// <summary>The spot's size category.</summary>
	public SpotSize Size { get; set; }
}
