using ParkingApp.Shared.Parking;
using ParkingApp.Shared.Responses;

namespace ParkingApp.Api.Services.Parking
{
	public interface IParkingEntryService
	{
		Task<ApiResponse<List<ParkingEntryDto>>> GetAllAsync();
		Task<ApiResponse<ParkingEntryDto>> GetByIdAsync(int id);
		Task<ApiResponse<ParkingEntryDto>> CreateAsync(CreateParkingEntryRequest request);
		Task<ApiResponse<ParkingEntryDto>> UpdateAsync(int id, UpdateParkingEntryRequest request);
		Task<ApiResponse<bool>> DeleteAsync(int id);
	}
}
