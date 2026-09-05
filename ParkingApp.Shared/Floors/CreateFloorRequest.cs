using System.ComponentModel.DataAnnotations;

namespace ParkingApp.Shared.Floors;

/// <summary>
/// Payload sent by the client to create a new floor within a branch.
/// The branch is identified by the route, not this body.
/// </summary>
public class CreateFloorRequest
{
	/// <summary>The floor type to create.</summary>
	[Required]
	public FloorType Type { get; set; }
}
