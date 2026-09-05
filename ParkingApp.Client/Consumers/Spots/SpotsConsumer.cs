using System.Net.Http.Headers;
using System.Net.Http.Json;
using ParkingApp.Client.Services.Auth;
using ParkingApp.Shared.Responses;
using ParkingApp.Shared.Spots;

namespace ParkingApp.Client.Consumers.Spots;

/// <summary>
/// Default <see cref="ISpotsConsumer"/>: calls the nested /api/floors/{floorId}/spots
/// endpoints, attaching the stored JWT as a Bearer token.
/// </summary>
public class SpotsConsumer : ISpotsConsumer
{
	private readonly HttpClient _httpClient;

	public SpotsConsumer(HttpClient httpClient)
	{
		_httpClient = httpClient;
	}

	/// <inheritdoc />
	public async Task<ApiResponse<List<SpotDto>>?> GetByFloorAsync(Guid floorId)
	{
		return await _httpClient
			.GetFromJsonAsync<ApiResponse<List<SpotDto>>>($"api/floors/{floorId}/spots");
	}

	/// <inheritdoc />
	public async Task<ApiResponse<List<SpotDto>>?> GenerateAsync(
		Guid floorId, GenerateSpotsRequest request)
	{
		var response = await _httpClient
			.PostAsJsonAsync($"api/floors/{floorId}/spots/generate", request);

		return await response.Content.ReadFromJsonAsync<ApiResponse<List<SpotDto>>>();
	}

}