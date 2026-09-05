using System.ComponentModel.DataAnnotations;

namespace ParkingApp.Shared.Spots;

/// <summary>
/// Payload to create a spot within a floor. The floor is identified by the route.
/// </summary>
public class CreateSpotRequest
{
	/// <summary>The spot's code/number.</summary>
	[Required]
	[MaxLength(20)]
	public required string Number { get; set; }

	/// <summary>The spot's size category.</summary>
	[Required]
	public SpotSize Size { get; set; }
}
