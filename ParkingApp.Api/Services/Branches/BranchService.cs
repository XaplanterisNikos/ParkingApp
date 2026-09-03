using Microsoft.EntityFrameworkCore;
using ParkingApp.Api.Data;
using ParkingApp.Api.Data.Entities;
using ParkingApp.Api.MultiTenancy;
using ParkingApp.Shared.Branches;

namespace ParkingApp.Api.Services.Branches;

/// <summary>
/// Default <see cref="IBranchService"/>, backed by <see cref="ParkingDbContext"/>.
/// Reads are tenant-scoped automatically by the global query filter; writes set the
/// tenant explicitly from <see cref="ITenantProvider"/>.
/// </summary>
public class BranchService : IBranchService
{
	private readonly ParkingDbContext _dbContext;
	private readonly ITenantProvider _tenantProvider;

	public BranchService(ParkingDbContext dbContext, ITenantProvider tenantProvider)
	{
		_dbContext = dbContext;
		_tenantProvider = tenantProvider;
	}

	/// <inheritdoc />
	public async Task<BranchDto> CreateAsync(CreateBranchRequest request)
	{
		var companyId = _tenantProvider.CurrentCompanyId
			?? throw new InvalidOperationException("No tenant context for creating a branch");

		var branch = new Branch
		{ Name = request.Name , CompanyId = companyId };

		_dbContext.Branches.Add(branch);
		await _dbContext.SaveChangesAsync();

		return new BranchDto { Id = branch.Id, Name = branch.Name };
	}

	/// <inheritdoc />
	public async Task<List<BranchDto>> GetAllAsync()
	{
		// No manual "Where(CompanyId == ...)" — the global query filter adds it for us.
		// AsNoTracking: this is a read-only list, so skip change tracking for speed.
		return await _dbContext.Branches
			.AsNoTracking()
			.OrderBy(branch => branch.Name)
			.Select(branch => new BranchDto
			{
				Id = branch.Id,
				Name = branch.Name
			})
			.ToListAsync();
	}

	/// <inheritdoc />
	public async Task<BranchDto?> GetByIdAsync(Guid branchId)
	{
		return await _dbContext.Branches
			.AsNoTracking()
			.Where(branch=> branch.Id == branchId)
			.Select(branch=>new BranchDto { Name = branch.Name,Id = branch.Id })
			.FirstOrDefaultAsync();
	}
}
