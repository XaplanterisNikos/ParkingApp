using ParkingApp.Shared.Console;

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
}