using ParkingApp.Shared.Companies;

namespace ParkingApp.Api.Services.Companies;
/// <summary>
/// Read/query operations for companies (tenants).
/// </summary>
public interface ICompanyService
{
	/// <summary>
	/// Gets a single company by its id, projected to a <see cref="CompanyDto"/>.
	/// </summary>
	/// <param name="companyId">The company's id (taken from the caller's token).</param>
	/// <returns>The company DTO, or null if no company with that id exists.</returns>
	Task<CompanyDto?> GetByIdAsync(Guid companyId);
}
