using Microsoft.IdentityModel.Tokens;
using ParkingApp.Client.Services.Auth;
using ParkingApp.Shared.Branches;
using ParkingApp.Shared.Responses;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mail;

namespace ParkingApp.Client.Consumers.Branches;

/// <summary>
/// Default <see cref="IBranchesConsumer"/>: calls the protected branch endpoints,
/// attaching the stored JWT as a Bearer token.
/// </summary>
public class BranchesConsumer : IBranchesConsumer
{
	private readonly HttpClient _httpClient;

	public BranchesConsumer(HttpClient httpClient, ITokenStore tokenStore)
	{
		_httpClient = httpClient;
	}

	/// <inheritdoc />
	public async Task<ApiResponse<BranchDto>?> CreateAsync(CreateBranchRequest request)
	{
		var response = await _httpClient.PostAsJsonAsync("api/branches", request);
		return await response.Content.ReadFromJsonAsync<ApiResponse<BranchDto>>();
	}

	/// <inheritdoc />
	public async Task<ApiResponse<List<BranchDto>>?> GetAllAsync()
	{
		return await _httpClient
			.GetFromJsonAsync<ApiResponse<List<BranchDto>>>("api/branches");
	}

	/// <inheritdoc />
	public async Task<ApiResponse<BranchDto>?> GetByIdAsync(Guid branchId)
	{
		return await _httpClient
			.GetFromJsonAsync<ApiResponse<BranchDto>>($"api/branches/{branchId}");
	}

}
