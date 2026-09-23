using ParkingApp.Shared.Console;
using ParkingApp.Shared.Responses;
using ParkingApp.Shared.Spots;

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

	/// <summary>
	/// Gets the spot map of the active branch (floors, spots, live occupancy) and, when
	/// <paramref name="size"/> is given, the server's suggested free spot of exactly that size.
	/// </summary>
	/// <param name="size">
	/// The vehicle's size category, or <c>null</c> to get the map without a suggestion.
	/// </param>
	Task<ApiResponse<SpotMapDto>?> GetSpotMapAsync(SpotSize? size);
}