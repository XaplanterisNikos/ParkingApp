using ParkingApp.Shared.Branches;

namespace ParkingApp.Api.Services.Branches;

/// <summary>
/// Operations on the current tenant's branches.
/// </summary>
public interface IBranchService
{
	#region Get Methods
	/// <summary>Gets a single branch by id, or null if it doesn't exist for this tenant.</summary>
	Task<BranchDto?> GetByIdAsync(Guid branchId);

	/// <summary>Returns all branches belonging to the current tenant.</summary>
	Task<List<BranchDto>> GetAllAsync();
	#endregion

	/// <summary>Creates a new branch for the current tenant.</summary>
	/// <param name="request">The branch to create.</param>
	/// <returns>The created branch.</returns>
	Task<BranchDto> CreateAsync(CreateBranchRequest request);

	
}
