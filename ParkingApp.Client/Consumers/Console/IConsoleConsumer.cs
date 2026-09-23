using ParkingApp.Shared.Console;
using ParkingApp.Shared.Responses;

namespace ParkingApp.Client.Consumers.Console;

/// <summary>
/// Calls the employee console read endpoints. The active branch travels inside the
/// JWT (attached by AuthTokenHandler), so no branch id is ever sent from here.
/// </summary>
public interface IConsoleConsumer
{
	/// <summary>
	/// Gets total and occupied spots per size category for the active branch.
	/// </summary>
	Task<ApiResponse<List<SpotSizeOccupancyDto>>?> GetOccupancyAsync();
}