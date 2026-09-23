using ParkingApp.Shared.Spots;

namespace ParkingApp.Shared.Console;

/// <summary>
/// Occupancy of one spot size category in the employee's active branch.
/// Only sizes that actually have spots in the branch are returned.
/// </summary>
public class SpotSizeOccupancyDto
{
	/// <summary>The spot size category.</summary>
	public SpotSize Size { get; set; }

	/// <summary>How many spots of this size exist in the branch.</summary>
	public int Total { get; set; }

	/// <summary>How many of them currently have an active ticket.</summary>
	public int Occupied { get; set; }
}