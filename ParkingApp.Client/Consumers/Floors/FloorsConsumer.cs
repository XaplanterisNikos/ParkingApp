using ParkingApp.Client.Services.Auth;
using ParkingApp.Shared.Floors;
using ParkingApp.Shared.Responses;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ParkingApp.Client.Consumers.Floors;

/// <summary>
/// Default <see cref="IFloorsConsumer"/>: calls the nested /api/branches/{branchId}/floors
/// endpoints, attaching the stored JWT as a Bearer token.
/// </summary>
public class FloorsConsumer : IFloorsConsumer
{
	private readonly HttpClient _httpClient;

	public FloorsConsumer(HttpClient httpClient)
	{
		_httpClient = httpClient;
	}

	/// <inheritdoc />
	public async Task<ApiResponse<FloorDto>?> CreateAsync(Guid branchId, CreateFloorRequest request)
	{
		var response = await _httpClient
			.PostAsJsonAsync($"api/branches/{branchId}/floors", request);

		return await response.Content.ReadFromJsonAsync<ApiResponse<FloorDto>>();
	}

	/// <inheritdoc />
	public async Task<ApiResponse<List<FloorDto>>?> GetByBranchAsync(Guid branchId)
	{
		return await _httpClient
			.GetFromJsonAsync<ApiResponse<List<FloorDto>>>($"api/branches/{branchId}/floors");
	}

}
