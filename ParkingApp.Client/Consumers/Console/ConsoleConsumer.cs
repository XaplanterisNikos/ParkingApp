using System.Net.Http.Json;
using ParkingApp.Shared.Console;
using ParkingApp.Shared.Responses;

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
}