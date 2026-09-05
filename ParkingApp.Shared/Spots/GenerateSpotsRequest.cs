using System.ComponentModel.DataAnnotations;

namespace ParkingApp.Shared.Spots;

/// <summary>
/// Payload to generate a batch of spots of one size on a floor.
/// </summary>
public class GenerateSpotsRequest
{
	/// <summary>The size category for all generated spots.</summary>
	[Required]
	public SpotSize Size { get; set; }

	/// <summary>How many spots to generate.</summary>
	[Range(1, 1000)]
	public int Count { get; set; }
}