using ParkingApp.Shared.Branches;

namespace ParkingApp.Api.Services.Branches;

/// <summary>
/// Operations on the current tenant's branches.
/// </summary>
public interface IBranchService
{
	/// <summary>Returns all branches belonging to the current tenant.</summary>
	Task<List<BranchDto>> GetAllAsync();

	/// <summary>Creates a new branch for the current tenant.</summary>
	/// <param name="request">The branch to create.</param>
	/// <returns>The created branch.</returns>
	Task<BranchDto> CreateAsync(CreateBranchRequest request);
}
