using ParkingApp.Shared.Console;
using ParkingApp.Shared.Spots;

namespace ParkingApp.Api.Services.Console;

/// <summary>
/// Read-side of the employee console: what the cash desk sees about its branch.
/// Always scoped to the active branch from the caller's token — callers cannot
/// choose a different branch.
/// </summary>
public interface IConsoleService
{
	/// <summary>
	/// Returns total and occupied spots per size category for the active branch.
	/// Only sizes that have at least one spot in the branch are included.
	/// </summary>
	/// <exception cref="InvalidOperationException">
	/// The caller's token has no active branch — the endpoint must be protected
	/// by the <c>ActiveBranch</c> policy.
	/// </exception>
	Task<List<SpotSizeOccupancyDto>> GetOccupancyAsync();

	/// <summary>
	/// Returns the spot map of the active branch: every floor (in physical order) with its
	/// spots (in display order) and whether each one currently has an active ticket.
	/// When <paramref name="size"/> is given, also proposes the first free spot of exactly
	/// that size (ground floor first, then by distance from it). Nothing is reserved.
	/// </summary>
	/// <param name="size">
	/// The vehicle's size category, or <c>null</c> to get the map without a suggestion.
	/// </param>
	/// <exception cref="InvalidOperationException">
	/// The caller's token has no active branch — the endpoint must be protected
	/// by the <c>ActiveBranch</c> policy.
	/// </exception>
	Task<SpotMapDto> GetSpotMapAsync(SpotSize? size);
}