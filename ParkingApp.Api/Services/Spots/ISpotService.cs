using ParkingApp.Shared.Spots;

namespace ParkingApp.Api.Services.Spots;

/// <summary>
/// Operations on parking spots within a floor (scoped to the current tenant).
/// </summary>
public interface ISpotService
{
	/// <summary>Returns all spots of the given floor.</summary>
	Task<List<SpotDto>> GetByFloorAsync(Guid floorId);

	/// <summary>
	/// Generates a batch of spots of the given size on the given floor, auto-numbered
	/// continuing from any existing spots of that size. Returns null if the floor
	/// doesn't exist for this tenant.
	/// </summary>
	Task<List<SpotDto>?> GenerateAsync(Guid floorId, GenerateSpotsRequest request);
}
