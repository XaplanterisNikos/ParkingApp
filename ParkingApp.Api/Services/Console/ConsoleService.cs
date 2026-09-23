using Microsoft.EntityFrameworkCore;
using ParkingApp.Api.Data;
using ParkingApp.Api.MultiTenancy;
using ParkingApp.Shared.Console;
using ParkingApp.Shared.Spots;

namespace ParkingApp.Api.Services.Console;

/// <summary>
/// Default <see cref="IConsoleService"/>. Every query is filtered explicitly by the
/// active branch (from <see cref="IActiveBranchProvider"/>); the tenant filter is
/// applied automatically by the DbContext.
/// </summary>
public class ConsoleService : IConsoleService
{
	// Database access; tenant (company) filtering is automatic via global query filters
	private readonly ParkingDbContext _dbContext;

	// The branch the employee works in — the ONLY source of the branch id
	private readonly IActiveBranchProvider _activeBranchProvider;

	/// <summary>
	/// Creates the console service.
	/// </summary>
	/// <param name="dbContext">The application's database context.</param>
	/// <param name="activeBranchProvider">Provides the caller's active branch from the token.</param>
	public ConsoleService(ParkingDbContext dbContext, IActiveBranchProvider activeBranchProvider)
	{
		_dbContext = dbContext;
		_activeBranchProvider = activeBranchProvider;
	}

	/// <inheritdoc />
	public async Task<List<SpotSizeOccupancyDto>> GetOccupancyAsync()
	{
		// Second line of defence: the ActiveBranch policy should make this impossible
		var branchId = _activeBranchProvider.CurrentBranchId
			?? throw new InvalidOperationException(
				"No active branch in the current token. Protect the endpoint with the ActiveBranch policy.");

		return await _dbContext.ParkingSpots
			.AsNoTracking()
			// Spots have no BranchId: reach the branch through the spot's floor
			.Where(spot => _dbContext.Floors
				.Any(floor => floor.Id == spot.FloorId && floor.BranchId == branchId))
			// Keep only what we count: the size, and whether an active ticket exists
			.Select(spot => new
			{
				spot.Size,
				IsOccupied = _dbContext.ParkingTickets
					.Any(ticket => ticket.ParkingSpotId == spot.Id && ticket.ExitedAt == null)
			})
			.GroupBy(spot => spot.Size)
			.Select(group => new SpotSizeOccupancyDto
			{
				Size = group.Key,
				Total = group.Count(),
				Occupied = group.Count(spot => spot.IsOccupied)
			})
			// Stable card order in the UI (Motorcycle, Car, LargeCar)
			.OrderBy(dto => dto.Size)
			.ToListAsync();
	}

	/// <inheritdoc />
	public async Task<SpotMapDto> GetSpotMapAsync(SpotSize? size)
	{
		// Second line of defence: the ActiveBranch policy should make this impossible
		var branchId = _activeBranchProvider.CurrentBranchId
			?? throw new InvalidOperationException(
				"No active branch in the current token. Protect the endpoint with the ActiveBranch policy.");

		// Query 1: every floor of the branch — floors without spots must still appear on the map
		var floors = await _dbContext.Floors
			.AsNoTracking()
			.Where(floor => floor.BranchId == branchId)
			.Select(floor => new { floor.Id, floor.Type })
			.ToListAsync();

		// Query 2: every spot of the branch with its live occupancy (same rule as GetOccupancyAsync)
		var spots = await _dbContext.ParkingSpots
			.AsNoTracking()
			// Spots have no BranchId: reach the branch through the spot's floor
			.Where(spot => _dbContext.Floors
				.Any(floor => floor.Id == spot.FloorId && floor.BranchId == branchId))
			.Select(spot => new
			{
				// Kept only to attach the spot to its floor in memory; not part of the DTO
				spot.FloorId,
				Status = new SpotStatusDto
				{
					Id = spot.Id,
					Number = spot.Number,
					Size = spot.Size,
					// Occupied = an active ticket exists (ExitedAt IS NULL); nothing is stored on the spot
					IsOccupied = _dbContext.ParkingTickets
						.Any(ticket => ticket.ParkingSpotId == spot.Id && ticket.ExitedAt == null)
				}
			})
			.ToListAsync();

		// In memory from here on: a few hundred rows at most, and C# ordering needs no SQL translation.
		// A lookup returns an EMPTY sequence for a missing key — exactly what a floor without spots needs.
		var spotsByFloor = spots.ToLookup(row => row.FloorId, row => row.Status);

		var floorMaps = floors
			// Physical order: lowest basement first (enum values -2 ... 10)
			.OrderBy(floor => floor.Type)
			.Select(floor => new FloorMapDto
			{
				Type = floor.Type,
				Spots = spotsByFloor[floor.Id]
					// Size first: the number prefix (floor code + size code) is constant only within one size...
					.OrderBy(spot => spot.Size)
					// ...so within a size, shorter numbers come first (AC2 before AC10)...
					.ThenBy(spot => spot.Number.Length)
					// ...and equal lengths compare character by character, independent of culture
					.ThenBy(spot => spot.Number, StringComparer.Ordinal)
					.ToList()
			})
			.ToList();

		// Suggestion: the first FREE spot of EXACTLY the vehicle's size. A proposal only — nothing is
		// reserved, so two desks asking at the same moment get the same spot (the loser gets a 409 on entry).
		var suggestedSpotId = size is null
			? null
			: floorMaps
				// Floor priority: ground first, then by distance from it; on a tie the basement wins
				// (0, -1, 1, -2, 2, ...). This re-orders a copy — floorMaps keeps its physical order.
				.OrderBy(floor => Math.Abs((int)floor.Type))
				.ThenBy(floor => floor.Type)
				// Flatten while keeping order: floor by floor, each floor's spots in display order
				// (with an exact size the number prefix is constant, so AC2 comes before AC10)
				.SelectMany(floor => floor.Spots)
				.FirstOrDefault(spot => spot.Size == size && !spot.IsOccupied)
				// No match → null → null id (see the note on Guid.Empty below)
				?.Id;

		return new SpotMapDto { Floors = floorMaps, SuggestedSpotId = suggestedSpotId };
	}
}