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
	private readonly ITokenStore _tokenStore;

	public BranchesConsumer(HttpClient httpClient, ITokenStore tokenStore)
	{
		_httpClient = httpClient;
		_tokenStore = tokenStore;
	}

	/// <inheritdoc />
	public async Task<ApiResponse<BranchDto>?> CreateAsync(CreateBranchRequest request)
	{
		var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/branches")
		{
			Content = JsonContent.Create(request)
		};

		await AttachTokenAsync(httpRequest);

		var response = await _httpClient.SendAsync(httpRequest);
		return await response.Content.ReadFromJsonAsync<ApiResponse<BranchDto>>();
	}

	/// <inheritdoc />
	public async Task<ApiResponse<List<BranchDto>>?> GetAllAsync()
	{
		var request = new HttpRequestMessage(HttpMethod.Get, "api/branches");
		await AttachTokenAsync(request);

		var response = await _httpClient.SendAsync(request);
		return await response.Content.ReadFromJsonAsync<ApiResponse<List<BranchDto>>>();
	}

	/// <summary>Attaches the stored JWT to the request as a Bearer token.</summary>
	private async Task AttachTokenAsync(HttpRequestMessage request)
	{
		var token = await _tokenStore.GetTokenAsync();
		if(!string.IsNullOrWhiteSpace(token))
		{
			request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
		}
	}

	/// <inheritdoc />
	public async Task<ApiResponse<BranchDto>?> GetByIdAsync(Guid branchId)
	{
		var request = new HttpRequestMessage(HttpMethod.Get, $"api/branches/{branchId}");
		await AttachTokenAsync(request);

		var response = await _httpClient.SendAsync(request);
		return await response.Content.ReadFromJsonAsync<ApiResponse<BranchDto>>();
	}

}
