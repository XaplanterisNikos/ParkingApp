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

	/// <inheritdoc />
	public async Task<ApiResponse<List<TicketDto>>?> GetActiveAsync() =>
		// Throws HttpRequestException on non-2xx — the view model catches it
		await _httpClient.GetFromJsonAsync<ApiResponse<List<TicketDto>>>("api/tickets/active");

	/// <inheritdoc />
	public async Task<ApiResponse<ExitQuoteDto>?> GetQuoteAsync(Guid ticketId)
	{
		// GetAsync (not GetFromJsonAsync): 404 and 409 carry a message the employee must see
		var response = await _httpClient.GetAsync($"api/tickets/{ticketId}/quote");
		return await response.Content.ReadFromJsonAsync<ApiResponse<ExitQuoteDto>>();
	}

	/// <inheritdoc />
	public async Task<ApiResponse<TicketDto>?> ExitAsync(Guid ticketId, ExitTicketRequest request)
	{
		var response = await _httpClient.PostAsJsonAsync($"api/tickets/{ticketId}/exit", request);
		return await response.Content.ReadFromJsonAsync<ApiResponse<TicketDto>>();
	}
}
