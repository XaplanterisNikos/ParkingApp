namespace ParkingApp.Shared.Console;

/// <summary>
/// The spot map of the employee's active branch: every floor with its spots and their
/// current occupancy, plus an optional spot suggestion for the vehicle size requested.
/// The suggestion is only a proposal — nothing is reserved; the employee confirms or picks another.
/// </summary>
public class SpotMapDto
{
	/// <summary>
	/// The branch's floors in physical order (lowest basement first, highest floor last),
	/// ready to be shown as tabs or a vertical stack.
	/// </summary>
	public List<FloorMapDto> Floors { get; set; } = new();

	/// <summary>
	/// The spot the server proposes for the requested vehicle size: the first free spot of
	/// exactly that size (ground floor first, then by distance from it).
	/// <c>null</c> when no size was requested, or when no free spot of exactly that size exists
	/// — the employee can still pick a larger free spot manually.
	/// </summary>
	public Guid? SuggestedSpotId { get; set; }
}