using ParkingApp.Shared.Parking;
using ParkingApp.Shared.Responses;

namespace ParkingApp.Client.Consumers.Parking
{
	public interface IParkingEntriesConsumer
	{
		Task<ApiResponse<List<ParkingEntryDto>>> GetAllParkingEntriesAsync(bool includeDeleted=false);
		Task<ApiResponse<ParkingEntryDto>> GetParkingEntryByIdAsync(int id,bool includeDeleted=false);
		Task<ApiResponse<ParkingEntryDto>> CreateParkingEntryAsync(CreateParkingEntryRequest request);
		Task<ApiResponse<ParkingEntryDto>> UpdateParkingEntryAsync(int id, UpdateParkingEntryRequest request);
		Task<ApiResponse<bool>> DeleteParkingEntryAsync(int id);
	}
}
