using ParkingApp.Shared.Responses;
using ParkingApp.Shared.Spots;

namespace ParkingApp.Client.Consumers.Spots;

/// <summary>
/// Talks to the API's nested spot endpoints (spots live under a floor).
/// </summary>
public interface ISpotsConsumer
{
	/// <summary>Gets all spots of the given floor.</summary>
	Task<ApiResponse<List<SpotDto>>?> GetByFloorAsync(Guid floorId);

	/// <summary>Creates a new spot within the given floor.</summary>
	Task<ApiResponse<SpotDto>?> CreateAsync(Guid floorId, CreateSpotRequest request);
}
