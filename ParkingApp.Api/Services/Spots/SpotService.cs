using Microsoft.EntityFrameworkCore;
using ParkingApp.Api.Data;
using ParkingApp.Api.Data.Entities;
using ParkingApp.Api.MultiTenancy;
using ParkingApp.Shared.Spots;

namespace ParkingApp.Api.Services.Spots;

/// <summary>
/// Default <see cref="ISpotService"/>. Tenant scoping is automatic (global query
/// filter); the floor scoping is applied explicitly per operation.
/// </summary>
public class SpotService : ISpotService
{
	private readonly ParkingDbContext _dbContext;
	private readonly ITenantProvider _tenantProvider;

	public SpotService(ParkingDbContext dbContext, ITenantProvider tenantProvider)
	{
		_dbContext = dbContext;
		_tenantProvider = tenantProvider;
	}

	/// <inheritdoc />
	public async Task<SpotDto> CreateAsync(Guid floorId, CreateSpotRequest request)
	{
		var companyId = _tenantProvider.CurrentCompanyId
		   ?? throw new InvalidOperationException("No tenant context for creating a spot.");

		var spot = new ParkingSpot
		{
			Number = request.Number,
			Size = request.Size,
			FloorId = floorId,          // from the route
			CompanyId = companyId       // from the token
		};

		_dbContext.ParkingSpots.Add(spot);
		await _dbContext.SaveChangesAsync();

		return new SpotDto
		{
			Id = spot.Id,
			Number = spot.Number,
			Size = spot.Size
		};
	}

	/// <inheritdoc />
	public async Task<List<SpotDto>> GetByFloorAsync(Guid floorId)
	{
		// Tenant filter (CompanyId) is automatic; we add only the floor filter.
		return await _dbContext.ParkingSpots
			.AsNoTracking()
			.Where(spot => spot.FloorId == floorId)
			.OrderBy(spot => spot.Number)
			.Select(spot => new SpotDto
			{
				Id = spot.Id,
				Number = spot.Number,
				Size = spot.Size
			})
			.ToListAsync();
	}
}
