using ParkingApp.Shared.Session;

namespace ParkingApp.Api.Services.Session;

/// <summary>
/// Manages the employee's work session: which branches they may work in, and
/// selecting the active one (which re-issues their token with a branch claim).
/// </summary>
public interface ISessionService
{
	/// <summary>Returns the branches the given employee is assigned to.</summary>
	Task<List<BranchOptionDto>> GetMyBranchesAsync(string userId);

	/// <summary>
	/// Verifies the employee is assigned to the branch and, if so, re-issues their
	/// JWT with the active branch embedded.
	/// </summary>
	/// <returns>Success flag, an error message on failure, and the new token on success.</returns>
	Task<(bool Success, string? Error, string? Token)> SelectBranchAsync(string userId, Guid branchId);
}
