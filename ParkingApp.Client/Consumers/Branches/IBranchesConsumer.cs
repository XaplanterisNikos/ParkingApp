using ParkingApp.Shared.Branches;
using ParkingApp.Shared.Responses;

namespace ParkingApp.Client.Consumers.Branches;

/// <summary>
/// Talks to the API's branch endpoints over HTTP.
/// </summary>
public interface IBranchesConsumer
{
	/// <summary>Gets all branches belonging to the current tenant.</summary>
	Task<ApiResponse<List<BranchDto>>?> GetAllAsync();

	/// <summary>Gets a single branch by id.</summary>
	Task<ApiResponse<BranchDto>?> GetByIdAsync(Guid branchId);

	/// <summary>Creates a new branch for the current tenant.</summary>
	Task<ApiResponse<BranchDto>?> CreateAsync(CreateBranchRequest request);
}
