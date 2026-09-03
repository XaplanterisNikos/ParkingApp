using ParkingApp.Shared.Floors;
using ParkingApp.Shared.Responses;

namespace ParkingApp.Client.Consumers.Floors;

/// <summary>
/// Talks to the API's nested floor endpoints (floors live under a branch).
/// </summary>
public interface IFloorsConsumer
{
	/// <summary>Gets all floors of the given branch.</summary>
	Task<ApiResponse<List<FloorDto>>?> GetByBranchAsync(Guid branchId);

	/// <summary>Creates a new floor within the given branch.</summary>
	Task<ApiResponse<FloorDto>?> CreateAsync(Guid branchId, CreateFloorRequest request);
}
