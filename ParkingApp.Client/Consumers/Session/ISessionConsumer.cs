using ParkingApp.Shared.Responses;
using ParkingApp.Shared.Session;

namespace ParkingApp.Client.Consumers.Session;

/// <summary>
/// Calls the employee session endpoints: listing the employee's branches and
/// selecting the active one.
/// </summary>
public interface ISessionConsumer
{
	/// <summary>Gets the branches the current employee is assigned to.</summary>
	Task<ApiResponse<List<BranchOptionDto>>?> GetMyBranchesAsync();

	/// <summary>Selects the active branch; the API returns a re-issued token.</summary>
	Task<ApiResponse<SelectBranchResponse>?> SelectBranchAsync(SelectBranchRequest request);
}