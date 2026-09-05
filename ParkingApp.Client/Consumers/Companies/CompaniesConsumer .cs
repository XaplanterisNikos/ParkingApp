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

	public CompaniesConsumer(HttpClient httpClient)
	{
		_httpClient = httpClient;
	}

	/// <inheritdoc />
	public async Task<ApiResponse<CompanyDto>?> GetMyCompanyAsync()
	{
		return await _httpClient
			.GetFromJsonAsync<ApiResponse<CompanyDto>>("api/companies/about");
	}
}
