using ParkingApp.Shared.Pricing;
using ParkingApp.Shared.Responses;
using System.Net.Http.Json;

namespace ParkingApp.Client.Consumers.Pricing;

/// <summary>
/// Default <see cref="IHolidaysConsumer"/>. Pure HTTP: the JWT is attached by AuthTokenHandler.
/// </summary>
public class HolidaysConsumer : IHolidaysConsumer
{
	// The "Api" named client, preconfigured with the base address and AuthTokenHandler
	private readonly HttpClient _httpClient;

	/// <summary>
	/// Creates the consumer.
	/// </summary>
	/// <param name="httpClient">The configured API client.</param>
	public HolidaysConsumer(HttpClient httpClient)
	{
		_httpClient = httpClient;
	}

	/// <inheritdoc />
	public async Task<ApiResponse<HolidayCalendarDto>?> GetCalendarAsync() =>
		// Throws HttpRequestException on non-2xx — the view model catches it
		await _httpClient.GetFromJsonAsync<ApiResponse<HolidayCalendarDto>>("api/holidays");

	/// <inheritdoc />
	public async Task<ApiResponse<HolidayDto>?> AddAsync(CreateHolidayRequest request)
	{
		var response = await _httpClient.PostAsJsonAsync("api/holidays", request);

		// 400 and 409 also carry an ApiResponse with the message
		return await response.Content.ReadFromJsonAsync<ApiResponse<HolidayDto>>();
	}

	/// <inheritdoc />
	public async Task<ApiResponse<bool>?> DeleteAsync(Guid holidayId)
	{
		var response = await _httpClient.DeleteAsync($"api/holidays/{holidayId}");
		return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
	}

	/// <inheritdoc />
	public async Task<ApiResponse<bool>?> SetSettingsAsync(HolidaySettingsRequest request)
	{
		var response = await _httpClient.PutAsJsonAsync("api/holidays/settings", request);
		return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
	}
}
