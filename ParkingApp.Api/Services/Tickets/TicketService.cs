using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ParkingApp.Api.Data;
using ParkingApp.Api.Data.Entities;
using ParkingApp.Api.MultiTenancy;
using ParkingApp.Shared.Spots;
using ParkingApp.Shared.Tickets;

namespace ParkingApp.Api.Services.Tickets;

/// <summary>
/// Default <see cref="ITicketService"/>. Rules that two desks can break at the same moment
/// (one active ticket per spot, one per plate) are enforced by unique indexes in the database;
/// this class only translates their errors into clear results.
/// </summary>
public class TicketService : ITicketService
{
	// SQL Server error numbers for a duplicate key: 2601 = unique index, 2627 = unique constraint.
	private const int DuplicateKeyIndexError = 2601;
	private const int DuplicateKeyConstraintError = 2627;

	// Index names from ParkingDbContext. The name tells which rule was broken.
	private const string ActivePerSpotIndex = "IX_ParkingTickets_ActivePerSpot";
	private const string ActivePerPlateIndex = "IX_ParkingTickets_ActivePerPlate";

	// Database access; tenant (company) filtering is automatic via global query filters
	private readonly ParkingDbContext _dbContext;

	// The company of the caller, needed to stamp the new ticket
	private readonly ITenantProvider _tenantProvider;

	// The branch the employee works in — the ONLY source of the branch id
	private readonly IActiveBranchProvider _activeBranchProvider;

	/// <summary>
	/// Creates the ticket service.
	/// </summary>
	/// <param name="dbContext">The application's database context.</param>
	/// <param name="tenantProvider">Provides the caller's company from the token.</param>
	/// <param name="activeBranchProvider">Provides the caller's active branch from the token.</param>
	public TicketService(
		ParkingDbContext dbContext,
		ITenantProvider tenantProvider,
		IActiveBranchProvider activeBranchProvider)
	{
		_dbContext = dbContext;
		_tenantProvider = tenantProvider;
		_activeBranchProvider = activeBranchProvider;
	}

	/// <inheritdoc />
	public async Task<EntryResult> EnterAsync(CreateEntryRequest request, string employeeId)
	{
		// Second line of defence: the ActiveBranch policy should make these impossible
		var companyId = _tenantProvider.CurrentCompanyId
			?? throw new InvalidOperationException("No tenant context for a vehicle entry.");
		var branchId = _activeBranchProvider.CurrentBranchId
			?? throw new InvalidOperationException(
				"No active branch in the current token. Protect the endpoint with the ActiveBranch policy.");

		// 1. Enum values come from the client, so they may be numbers that do not exist.
		if (!Enum.IsDefined(request.PlateKind))
		{
			return EntryResult.Failure(EntryOutcome.Invalid, "Άγνωστο είδος πινακίδας.");
		}

		if (!Enum.IsDefined(request.VehicleSize))
		{
			return EntryResult.Failure(EntryOutcome.Invalid, "Άγνωστο μέγεθος οχήματος.");
		}

		// 2. The plate: normalize and check it. The server's result is the one that counts.
		if (!LicensePlateNormalizer.TryNormalize(
				request.LicensePlate, request.PlateKind, out var plate, out var plateError))
		{
			return EntryResult.Failure(EntryOutcome.Invalid, plateError!);
		}

		// 3. The spot must belong to the ACTIVE branch. The tenant filter alone would also
		//    accept a spot of another branch of the same company, so we check the floor's branch.
		var spot = await _dbContext.ParkingSpots
			.AsNoTracking()
			.Where(candidate => candidate.Id == request.SpotId
				&& _dbContext.Floors.Any(floor => floor.Id == candidate.FloorId && floor.BranchId == branchId))
			.Select(candidate => new { candidate.Id, candidate.Number, candidate.Size })
			.FirstOrDefaultAsync();

		if (spot is null)
		{
			return EntryResult.Failure(EntryOutcome.SpotNotFound, "Η θέση δεν βρέθηκε σε αυτό το parking.");
		}

		// 4. Spot ≥ vehicle: the enum values grow with the size (Motorcycle 1 < Car 2 < LargeCar 3).
		if (spot.Size < request.VehicleSize)
		{
			return EntryResult.Failure(
				EntryOutcome.Invalid,
				$"Η θέση {spot.Number} είναι για {SpotSizeInfo.NameOf(spot.Size)} " +
				$"και δεν χωράει {SpotSizeInfo.NameOf(request.VehicleSize)}.");
		}

		// 5. Insert. We do NOT check first whether the spot is free: two desks could both pass
		//    such a check. The unique indexes decide, and the loser gets a clear 409.
		var ticket = new ParkingTicket
		{
			CompanyId = companyId,
			BranchId = branchId,
			ParkingSpotId = spot.Id,
			LicensePlate = plate,
			PlateKind = request.PlateKind,
			EnteredAt = DateTimeOffset.UtcNow,
			EnteredByEmployeeId = employeeId
		};

		_dbContext.ParkingTickets.Add(ticket);

		try
		{
			await _dbContext.SaveChangesAsync();
		}
		catch (DbUpdateException exception) when (IsDuplicateKey(exception, ActivePerSpotIndex))
		{
			return EntryResult.Failure(
				EntryOutcome.SpotTaken,
				$"Η θέση {spot.Number} μόλις πιάστηκε από άλλο ταμείο. Διάλεξε άλλη θέση.");
		}
		catch (DbUpdateException exception) when (IsDuplicateKey(exception, ActivePerPlateIndex))
		{
			return EntryResult.Failure(
				EntryOutcome.PlateAlreadyInside,
				$"Το όχημα {LicensePlateNormalizer.ToDisplay(plate, request.PlateKind)} είναι ήδη μέσα στο parking.");
		}

		return EntryResult.Success(new TicketDto
		{
			Id = ticket.Id,
			SpotId = spot.Id,
			SpotNumber = spot.Number,
			LicensePlate = ticket.LicensePlate,
			PlateKind = ticket.PlateKind,
			EnteredAt = ticket.EnteredAt
		});
	}

	/// <summary>
	/// Tells whether a save failed because of a duplicate key in the given unique index.
	/// </summary>
	/// <param name="exception">The exception thrown by SaveChanges.</param>
	/// <param name="indexName">The name of the unique index to look for.</param>
	private static bool IsDuplicateKey(DbUpdateException exception, string indexName) =>
		exception.InnerException is SqlException sqlException
		&& sqlException.Number is DuplicateKeyIndexError or DuplicateKeyConstraintError
		// SQL Server puts the index name in the message; it is the only place it appears.
		&& sqlException.Message.Contains(indexName, StringComparison.Ordinal);
}
