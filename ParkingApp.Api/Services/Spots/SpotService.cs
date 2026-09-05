using Microsoft.EntityFrameworkCore;
using ParkingApp.Api.Data;
using ParkingApp.Api.Data.Entities;
using ParkingApp.Api.MultiTenancy;
using ParkingApp.Shared.Floors;
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
	public async Task<List<SpotDto>?> GenerateAsync(Guid floorId, GenerateSpotsRequest request)
	{
		var companyId = _tenantProvider.CurrentCompanyId
			?? throw new InvalidOperationException("No tenant context for generating spots.");

		// 1. Find the floor (tenant filter makes a foreign floor come back null).
		var floor = await _dbContext.Floors
			.FirstOrDefaultAsync(f => f.Id == floorId);

		if (floor is null)
		{
			return null; // floor not found for this tenant
		}

		// 2. Count existing spots of this size to continue numbering.
		var existingCount = await _dbContext.ParkingSpots
			.CountAsync(spot => spot.FloorId == floorId && spot.Size == request.Size);

		// 3. Build the number prefix: floor code + size code (e.g. "AC").
		var prefix = FloorTypeInfo.CodeOf(floor.Type) + SpotSizeInfo.CodeOf(request.Size);

		// 4. Generate the batch.
		var newSpots = new List<ParkingSpot>();
		for (var i = 1; i <= request.Count; i++)
		{
			newSpots.Add(new ParkingSpot
			{
				Number = $"{prefix}{existingCount + i}",
				Size = request.Size,
				FloorId = floorId,
				CompanyId = companyId
			});
		}

		_dbContext.ParkingSpots.AddRange(newSpots);
		await _dbContext.SaveChangesAsync();   // one save = one transaction

		return newSpots
			.Select(spot => new SpotDto
			{
				Id = spot.Id,
				Number = spot.Number,
				Size = spot.Size
			})
			.ToList();
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
