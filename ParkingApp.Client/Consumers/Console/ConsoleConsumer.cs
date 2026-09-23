using ParkingApp.Shared.Console;
using ParkingApp.Shared.Responses;
using ParkingApp.Shared.Spots;
using System.Net.Http.Json;

namespace ParkingApp.Client.Consumers.Console;

/// <summary>
/// Default <see cref="IConsoleConsumer"/>. Pure HTTP: the stored JWT (with the
/// active branch claim) is attached by AuthTokenHandler, never by this class.
/// </summary>
public class ConsoleConsumer : IConsoleConsumer
{
	// The "Api" named client, preconfigured with the base address and AuthTokenHandler
	private readonly HttpClient _httpClient;

	/// <summary>
	/// Creates the consumer.
	/// </summary>
	/// <param name="httpClient">The configured API client.</param>
	public ConsoleConsumer(HttpClient httpClient)
	{
		_httpClient = httpClient;
	}

	/// <inheritdoc />
	public async Task<ApiResponse<List<SpotSizeOccupancyDto>>?> GetOccupancyAsync() =>
		// Throws HttpRequestException on non-2xx (e.g. 403, 500) — the view model catches it
		await _httpClient
			.GetFromJsonAsync<ApiResponse<List<SpotSizeOccupancyDto>>>("api/console/occupancy");

	/// <inheritdoc />
	public async Task<ApiResponse<SpotMapDto>?> GetSpotMapAsync(SpotSize? size)
	{
		// No size → plain map; with a size → the server also returns a suggestion.
		// The enum NAME is sent (e.g. "?size=Car"): it binds on the server and reads well in devtools.
		var url = size is null
			? "api/console/spot-map"
			: $"api/console/spot-map?size={size}";

		// Throws HttpRequestException on non-2xx (e.g. 400, 403, 500) — the view model catches it
		return await _httpClient.GetFromJsonAsync<ApiResponse<SpotMapDto>>(url);
	}
}