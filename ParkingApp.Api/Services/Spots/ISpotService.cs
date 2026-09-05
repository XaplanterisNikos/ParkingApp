using ParkingApp.Shared.Spots;

namespace ParkingApp.Api.Services.Spots;

/// <summary>
/// Operations on parking spots within a floor (scoped to the current tenant).
/// </summary>
public interface ISpotService
{
	/// <summary>Returns all spots of the given floor.</summary>
	Task<List<SpotDto>> GetByFloorAsync(Guid floorId);
	/// <summary>Creates a new spot within the given floor.</summary>
	Task<SpotDto> CreateAsync(Guid floorId, CreateSpotRequest request);
}
