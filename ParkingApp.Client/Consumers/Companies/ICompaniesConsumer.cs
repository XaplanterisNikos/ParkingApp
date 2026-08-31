using ParkingApp.Shared.Companies;
using ParkingApp.Shared.Responses;

namespace ParkingApp.Client.Consumers.Companies;

/// <summary>
/// Talks to the API's company endpoints over HTTP.
/// </summary>
public interface ICompaniesConsumer
{
	/// <summary>
	/// Gets the current user's own company (the token identifies which one).
	/// </summary>
	Task<ApiResponse<CompanyDto>?> GetMyCompanyAsync();
}
