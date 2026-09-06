using System.Net.Http.Json;
using ParkingApp.Shared.Responses;
using ParkingApp.Shared.Session;

namespace ParkingApp.Client.Consumers.Session;

/// <summary>
/// Default <see cref="ISessionConsumer"/>. The stored JWT is attached by the
/// AuthTokenHandler, so this consumer never touches the token itself.
/// </summary>
public class SessionConsumer : ISessionConsumer
{
	private readonly HttpClient _httpClient;

	public SessionConsumer(HttpClient httpClient)
	{
		_httpClient = httpClient;
	}

	/// <inheritdoc />
	public async Task<ApiResponse<List<BranchOptionDto>>?> GetMyBranchesAsync() =>
		await _httpClient
			.GetFromJsonAsync<ApiResponse<List<BranchOptionDto>>>("api/session/branches");

	/// <inheritdoc />
	public async Task<ApiResponse<SelectBranchResponse>?> SelectBranchAsync(SelectBranchRequest request)
	{
		var response = await _httpClient.PostAsJsonAsync("api/session/select-branch", request);
		return await response.Content.ReadFromJsonAsync<ApiResponse<SelectBranchResponse>>();
	}
}