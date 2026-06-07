using ParkingApp.Shared.Parking;
using ParkingApp.Shared.Responses;

namespace ParkingApp.Api.Services.Parking
{
	public interface IParkingEntryService
	{
		Task<ApiResponse<List<ParkingEntryDto>>> GetAllAsync(bool includeDeleted = false);
		Task<ApiResponse<ParkingEntryDto>> GetByIdAsync(int id, bool includeDeleted = false);
		Task<ApiResponse<ParkingEntryDto>> CreateAsync(CreateParkingEntryRequest request);
		Task<ApiResponse<ParkingEntryDto>> UpdateAsync(int id, UpdateParkingEntryRequest request);
		Task<ApiResponse<bool>> DeleteAsync(int id);
	}
}
