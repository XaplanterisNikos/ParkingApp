using ParkingApp.Shared.Floors;

namespace ParkingApp.Api.Services.Floors;

/// <summary>
/// Operations on floors within a branch (scoped to the current tenant).
/// </summary>
public interface IFloorService
{
	/// <summary>Returns all floors of the given branch.</summary>
	Task<List<FloorDto>> GetByBranchAsync(Guid branchId);

	/// <summary>Creates a new floor within the given branch.</summary>
	Task<FloorDto> CreateAsync(Guid branchId, CreateFloorRequest request);
}
