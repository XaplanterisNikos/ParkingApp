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
	private readonly ITokenStore _tokenStore;

	public FloorsConsumer(HttpClient httpClient, ITokenStore tokenStore)
	{
		_httpClient = httpClient;
		_tokenStore = tokenStore;
	}

	/// <inheritdoc />
	public async Task<ApiResponse<FloorDto>?> CreateAsync(Guid branchId, CreateFloorRequest request)
	{
		var httpRequest = new HttpRequestMessage(
			HttpMethod.Post, $"api/branches/{branchId}/floors")
		{
			Content = JsonContent.Create(request)
		};
		await AttachTokenAsync(httpRequest);

		var response = await _httpClient.SendAsync(httpRequest);
		return await response.Content.ReadFromJsonAsync<ApiResponse<FloorDto>>();
	}

	/// <inheritdoc />
	public async Task<ApiResponse<List<FloorDto>>?> GetByBranchAsync(Guid branchId)
	{
		var request = new HttpRequestMessage(
			HttpMethod.Get, $"api/branches/{branchId}/floors");
		await AttachTokenAsync(request);

		var resposne = await _httpClient.SendAsync(request);
		return await resposne.Content.ReadFromJsonAsync<ApiResponse<List<FloorDto>>>();
	}

	/// <summary>Attaches the stored JWT to the request as a Bearer token.</summary>
	private async Task AttachTokenAsync(HttpRequestMessage request)
	{
		var token = await _tokenStore.GetTokenAsync();
		if (!string.IsNullOrWhiteSpace(token))
		{
			request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
		}
	}
}
