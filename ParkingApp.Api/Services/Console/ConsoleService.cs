using Microsoft.EntityFrameworkCore;
using ParkingApp.Api.Data;
using ParkingApp.Api.MultiTenancy;
using ParkingApp.Shared.Console;

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
}