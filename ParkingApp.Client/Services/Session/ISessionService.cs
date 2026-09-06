using ParkingApp.Shared.Session;

namespace ParkingApp.Client.Services.Session;

/// <summary>
/// Client-side orchestration for the work session: loading the employee's branches
/// and selecting one (which stores the re-issued token and refreshes auth state).
/// </summary>
public interface ISessionService
{
	/// <summary>Loads the current employee's assigned branches.</summary>
	Task<(bool Success, string? Error, List<BranchOptionDto> Branches)> GetMyBranchesAsync();

	/// <summary>
	/// Selects the active branch. On success, stores the new token and notifies the
	/// auth state provider so the branch claim takes effect for later requests.
	/// </summary>
	Task<(bool Success, string? Error)> SelectBranchAsync(Guid branchId);
}