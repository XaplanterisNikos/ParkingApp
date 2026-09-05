using Microsoft.EntityFrameworkCore;
using ParkingApp.Api.Data;
using ParkingApp.Api.Data.Entities;
using ParkingApp.Api.MultiTenancy;
using ParkingApp.Shared.Floors;

namespace ParkingApp.Api.Services.Floors;

/// <summary>
/// Default <see cref="IFloorService"/>. Tenant scoping is automatic (global query
/// filter); the branch scoping is applied explicitly per operation.
/// </summary>
public class FloorService : IFloorService
{
	private readonly ParkingDbContext _dbContext;
	private readonly ITenantProvider _tenantProvider;

	public FloorService(ParkingDbContext dbContext, ITenantProvider tenantProvider)
	{
		_dbContext = dbContext;
		_tenantProvider = tenantProvider;
	}

	/// <inheritdoc />
	public async Task<FloorDto?> CreateAsync(Guid branchId, CreateFloorRequest request)
	{
		var companyId = _tenantProvider.CurrentCompanyId
			?? throw new InvalidOperationException("No tenant context for creating a floor.");

		// Uniqueness: a branch can have each floor type at most once.
		var exists = await _dbContext.Floors
			.AnyAsync(floor => floor.BranchId == branchId && floor.Type == request.Type);

		if (exists)
		{
			return null; // signals "already exists" to the controller
		}

		var floor = new Floor
		{
			Type = request.Type,
			BranchId = branchId,
			CompanyId = companyId
		};

		_dbContext.Floors.Add(floor);
		await _dbContext.SaveChangesAsync();

		return new FloorDto { Id = floor.Id, Type = floor.Type };
	}

	/// <inheritdoc />
	public async Task<List<FloorDto>> GetByBranchAsync(Guid branchId)
	{
		// Tenant filter (CompanyId) is added automatically by the global query filter.
		// We only add the branch filter — the business-level "which branch" scoping.
		return await _dbContext.Floors
			.AsNoTracking()
			.Where(floor=>floor.BranchId == branchId)
			.OrderBy(floor=>floor.Type)
			.Select(floor => new FloorDto
			{
				Id = floor.Id,
				Type = floor.Type
			})
			.ToListAsync();
	}
}
