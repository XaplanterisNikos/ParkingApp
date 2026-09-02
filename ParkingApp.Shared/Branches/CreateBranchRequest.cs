using System.ComponentModel.DataAnnotations;

namespace ParkingApp.Shared.Branches;

/// <summary>
/// Payload sent by the client to create a new branch.
/// </summary>
public class CreateBranchRequest
{
	/// <summary>The branch's display name.</summary>
	[Required]
	[MaxLength(200)]
	public required string Name { get; set; }
}
