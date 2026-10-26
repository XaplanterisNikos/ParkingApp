using ParkingApp.Shared.Responses;
using ParkingApp.Shared.Tickets;
using System.Net.Http.Json;

namespace ParkingApp.Client.Consumers.Tickets;

/// <summary>
/// Default <see cref="ITicketsConsumer"/>. Pure HTTP: the stored JWT (with the
/// active branch claim) is attached by AuthTokenHandler, never by this class.
/// </summary>
public class TicketsConsumer : ITicketsConsumer
{
	// The "Api" named client, preconfigured with the base address and AuthTokenHandler
	private readonly HttpClient _httpClient;

	/// <summary>
	/// Creates the consumer.
	/// </summary>
	/// <param name="httpClient">The configured API client.</param>
	public TicketsConsumer(HttpClient httpClient)
	{
		_httpClient = httpClient;
	}

	/// <inheritdoc />
	public async Task<ApiResponse<TicketDto>?> EnterAsync(CreateEntryRequest request)
	{
		var response = await _httpClient.PostAsJsonAsync("api/tickets/entry", request);

		// 400, 404 and 409 also carry an ApiResponse with a message, so the body is read
		// for every status. An empty body (e.g. 401, 403) throws; the view model catches it.
		return await response.Content.ReadFromJsonAsync<ApiResponse<TicketDto>>();
	}
}
