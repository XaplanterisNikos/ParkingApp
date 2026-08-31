using ParkingApp.Client.Services.Auth;
using ParkingApp.Shared.Companies;
using ParkingApp.Shared.Responses;
using System.Net.Http.Json;

namespace ParkingApp.Client.Consumers.Companies;

/// <summary>
/// Default <see cref="ICompaniesConsumer"/>: calls the protected company endpoint,
/// attaching the stored JWT as a Bearer token.
/// </summary>
public class CompaniesConsumer : ICompaniesConsumer
{
	private readonly HttpClient _httpClient;
	private readonly ITokenStore _tokenStore;

	public CompaniesConsumer(HttpClient httpClient, ITokenStore tokenStore)
	{
		_httpClient = httpClient;
		_tokenStore = tokenStore;
	}

	/// <inheritdoc />
	public async Task<ApiResponse<CompanyDto>?> GetMyCompanyAsync()
	{
		// Build the request and attach the JWT so the [Authorize] endpoint accepts it.
		var request = new HttpRequestMessage(HttpMethod.Get, "api/companies/about");

		var token = await _tokenStore.GetTokenAsync();
		if (!string.IsNullOrWhiteSpace(token)) 
		{
			request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
		}

		var response = await _httpClient.SendAsync(request);

		return await response.Content.ReadFromJsonAsync<ApiResponse<CompanyDto>>();
	}
}
