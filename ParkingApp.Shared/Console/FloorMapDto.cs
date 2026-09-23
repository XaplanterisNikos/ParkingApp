using ParkingApp.Shared.Floors;

namespace ParkingApp.Shared.Console;

/// <summary>
/// One floor of the active branch on the employee console's spot map,
/// with its spots in display order.
/// </summary>
public class FloorMapDto
{
	/// <summary>
	/// The floor type (Ground, Basement1, ...). Unique within a branch,
	/// so it also identifies the floor on the map (e.g. as the tab key).
	/// </summary>
	public FloorType Type { get; set; }

	/// <summary>
	/// The floor's spots, already ordered by the server (AC1, AC2, ..., AC10).
	/// Empty when the floor has no spots yet — the floor is still returned.
	/// </summary>
	public List<SpotStatusDto> Spots { get; set; } = new();
}