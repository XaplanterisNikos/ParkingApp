using System.ComponentModel.DataAnnotations;

namespace ParkingApp.Shared.Floors;

/// <summary>
/// Payload sent by the client to create a new floor within a branch.
/// The branch is identified by the route, not this body.
/// </summary>
public class CreateFloorRequest
{
	/// <summary>The floor's display name.</summary>
	[Required]
	[MaxLength(100)]
	public required string Name { get; set; }
}
