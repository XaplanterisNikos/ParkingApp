using ParkingApp.Shared.Pricing;
using ParkingApp.Shared.Responses;
using System.Net.Http.Json;

namespace ParkingApp.Client.Consumers.Pricing;

/// <summary>
/// Default <see cref="ITariffsConsumer"/>. Pure HTTP: the JWT is attached by AuthTokenHandler.
/// </summary>
public class TariffsConsumer : ITariffsConsumer
{
	// The "Api" named client, preconfigured with the base address and AuthTokenHandler
	private readonly HttpClient _httpClient;

	/// <summary>
	/// Creates the consumer.
	/// </summary>
	/// <param name="httpClient">The configured API client.</param>
	public TariffsConsumer(HttpClient httpClient)
	{
		_httpClient = httpClient;
	}

	/// <inheritdoc />
	public async Task<ApiResponse<TariffDto>?> GetCurrentAsync(Guid branchId) =>
		// Throws HttpRequestException on non-2xx (e.g. 404) — the view model catches it
		await _httpClient.GetFromJsonAsync<ApiResponse<TariffDto>>($"api/branches/{branchId}/tariff");

	/// <inheritdoc />
	public async Task<ApiResponse<TariffDto>?> SaveAsync(Guid branchId, SaveTariffRequest request)
	{
		var response = await _httpClient.PostAsJsonAsync($"api/branches/{branchId}/tariff", request);

		// 400 and 404 also carry an ApiResponse, so the body is read for every status
		return await response.Content.ReadFromJsonAsync<ApiResponse<TariffDto>>();
	}
}
