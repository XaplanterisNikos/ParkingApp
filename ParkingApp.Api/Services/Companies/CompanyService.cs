using Microsoft.EntityFrameworkCore;
using ParkingApp.Api.Data;
using ParkingApp.Shared.Companies;

namespace ParkingApp.Api.Services.Companies;

/// <summary>
/// Default <see cref="ICompanyService"/>, backed by <see cref="ParkingDbContext"/>.
/// </summary>
public class CompanyService : ICompanyService
{
	private readonly ParkingDbContext _dbContext;

	public CompanyService(ParkingDbContext dbContext)
	{
		_dbContext = dbContext;
	}

	/// <inheritdoc />
	public async Task<CompanyDto?> GetByIdAsync(Guid companyId)
	{
		return await _dbContext.Companies
			.Where(company => company.Id == companyId)
			.Select(company => new CompanyDto
			{
				Id = company.Id,
				Name = company.Name
			})
			.FirstOrDefaultAsync();
	}
}
