using Microsoft.EntityFrameworkCore;
using ParkingApp.Api.Data;
using ParkingApp.Api.Data.Entities;
using ParkingApp.Api.MultiTenancy;
using ParkingApp.Api.Pricing;
using ParkingApp.Shared.Pricing;
using ParkingApp.Shared.Spots;
using ParkingApp.Shared.Tickets;

namespace ParkingApp.Api.Services.Tickets;

/// <summary>
/// Default <see cref="ITicketExitService"/>. Reads the ticket, the price list versions and the
/// holiday calendar, lets <see cref="TariffCalculator"/> do the arithmetic, and closes the ticket
/// with one conditional UPDATE, so two desks can never close the same ticket twice.
/// </summary>
public class TicketExitService : ITicketExitService
{
	// Database access; tenant (company) filtering is automatic via global query filters
	private readonly ParkingDbContext _dbContext;

	// The company of the caller (for its holiday setting)
	private readonly ITenantProvider _tenantProvider;

	// The branch the employee works in — the ONLY source of the branch id
	private readonly IActiveBranchProvider _activeBranchProvider;

	// The pricing rules (no database, no clock)
	private readonly TariffCalculator _calculator;

	/// <summary>
	/// Creates the service.
	/// </summary>
	/// <param name="dbContext">The application's database context.</param>
	/// <param name="tenantProvider">Provides the caller's company from the token.</param>
	/// <param name="activeBranchProvider">Provides the caller's active branch from the token.</param>
	/// <param name="calculator">Calculates charges from a price list.</param>
	public TicketExitService(
		ParkingDbContext dbContext,
		ITenantProvider tenantProvider,
		IActiveBranchProvider activeBranchProvider,
		TariffCalculator calculator)
	{
		_dbContext = dbContext;
		_tenantProvider = tenantProvider;
		_activeBranchProvider = activeBranchProvider;
		_calculator = calculator;
	}

	/// <inheritdoc />
	public async Task<List<TicketDto>> GetActiveAsync()
	{
		var branchId = ActiveBranchId();

		return await _dbContext.ParkingTickets
			.AsNoTracking()
			.Where(ticket => ticket.BranchId == branchId && ticket.ExitedAt == null)
			.OrderByDescending(ticket => ticket.EnteredAt)
			.Select(ticket => new TicketDto
			{
				Id = ticket.Id,
				SpotId = ticket.ParkingSpotId,
				// Small subqueries instead of navigations: the entities have no navigation properties
				SpotNumber = _dbContext.ParkingSpots
					.Where(spot => spot.Id == ticket.ParkingSpotId)
					.Select(spot => spot.Number)
					.First(),
				LicensePlate = ticket.LicensePlate,
				PlateKind = ticket.PlateKind,
				EnteredAt = ticket.EnteredAt,
				OvernightPackageName = _dbContext.OvernightPackages
					.Where(package => package.Id == ticket.OvernightPackageId)
					.Select(package => package.Name)
					.FirstOrDefault()
			})
			.ToListAsync();
	}

	/// <inheritdoc />
	public async Task<ExitResult<ExitQuoteDto>> GetQuoteAsync(Guid ticketId)
	{
		var (context, failure) = await LoadContextAsync(ticketId);

		if (context is null)
		{
			return ExitResult<ExitQuoteDto>.Failure(failure!.Value.Outcome, failure.Value.Message);
		}

		var quotedAt = DateTimeOffset.UtcNow;

		try
		{
			var options = BuildOptions(context, quotedAt);

			if (options.Count == 0)
			{
				return ExitResult<ExitQuoteDto>.Failure(ExitOutcome.NoPriceList, NoPriceListMessage);
			}

			return ExitResult<ExitQuoteDto>.Success(new ExitQuoteDto
			{
				Ticket = context.ToDto(),
				QuotedAt = quotedAt,
				Options = options
			});
		}
		catch (InvalidOperationException)
		{
			// The calculator found no prices for this spot size in the price list
			return ExitResult<ExitQuoteDto>.Failure(ExitOutcome.NoPriceList, MissingSizeMessage(context.SpotSize));
		}
	}

	/// <inheritdoc />
	public async Task<ExitResult<TicketDto>> ExitAsync(Guid ticketId, ExitTicketRequest request, string employeeId)
	{
		var (context, failure) = await LoadContextAsync(ticketId);

		if (context is null)
		{
			return ExitResult<TicketDto>.Failure(failure!.Value.Outcome, failure.Value.Message);
		}

		// Only the versions that the quote offers are allowed: the entry one and the current one
		var version = context.Versions.FirstOrDefault(item => item.Id == request.TariffVersionId);

		if (version is null || (request.Overstay is { } overstay && !Enum.IsDefined(overstay)))
		{
			return ExitResult<TicketDto>.Failure(
				ExitOutcome.InvalidChoice, "Η επιλογή χρέωσης δεν ισχύει πλέον. Ανανεώστε την προεπισκόπηση.");
		}

		// The amount is calculated again NOW: the client never sends an amount
		var exitedAt = DateTimeOffset.UtcNow;
		decimal amount;

		try
		{
			var charged = Charge(context, version, request.Overstay ?? PackageOverstay.PriceList, exitedAt);

			if (charged is null)
			{
				return ExitResult<TicketDto>.Failure(
					ExitOutcome.InvalidChoice, "Το πακέτο δεν υπάρχει σε αυτή την έκδοση του τιμοκαταλόγου.");
			}

			amount = charged.Value;
		}
		catch (InvalidOperationException)
		{
			return ExitResult<TicketDto>.Failure(ExitOutcome.NoPriceList, MissingSizeMessage(context.SpotSize));
		}

		var branchId = ActiveBranchId();

		// One conditional UPDATE: it only touches the ticket while it is still active in this branch.
		// If another desk closed it a moment ago, 0 rows change and nothing is charged twice.
		// (Later: the invoice for Prisma will be queued here, in the same transaction.)
		var updated = await _dbContext.ParkingTickets
			.Where(ticket => ticket.Id == ticketId && ticket.BranchId == branchId && ticket.ExitedAt == null)
			.ExecuteUpdateAsync(setters => setters
				// The columns are nullable, so the values are cast to match their types exactly
				.SetProperty(ticket => ticket.ExitedAt, (DateTimeOffset?)exitedAt)
				.SetProperty(ticket => ticket.ExitedByEmployeeId, employeeId)
				.SetProperty(ticket => ticket.AmountCharged, (decimal?)amount)
				.SetProperty(ticket => ticket.VatRate, (decimal?)version.VatRate));

		if (updated == 0)
		{
			return ExitResult<TicketDto>.Failure(ExitOutcome.AlreadyExited, AlreadyExitedMessage);
		}

		var closed = context.ToDto();
		closed.ExitedAt = exitedAt;
		closed.AmountCharged = amount;
		closed.VatRate = version.VatRate;

		return ExitResult<TicketDto>.Success(closed);
	}

	#region Loading

	/// <summary>
	/// Loads everything an exit needs: the ticket (only if active in this branch), the price list
	/// versions it may be charged with, and the company's holiday calendar.
	/// </summary>
	private async Task<(ExitContext? Context, (ExitOutcome Outcome, string Message)? Failure)> LoadContextAsync(Guid ticketId)
	{
		var branchId = ActiveBranchId();

		var ticket = await _dbContext.ParkingTickets
			.AsNoTracking()
			.Where(item => item.Id == ticketId && item.BranchId == branchId)
			.Select(item => new
			{
				item.Id,
				item.ParkingSpotId,
				item.LicensePlate,
				item.PlateKind,
				item.EnteredAt,
				item.ExitedAt,
				item.TariffVersionId,
				item.OvernightPackageId,
				Spot = _dbContext.ParkingSpots
					.Where(spot => spot.Id == item.ParkingSpotId)
					.Select(spot => new { spot.Number, spot.Size })
					.First()
			})
			.FirstOrDefaultAsync();

		if (ticket is null)
		{
			return (null, (ExitOutcome.TicketNotFound, "Το εισιτήριο δεν βρέθηκε σε αυτό το parking."));
		}

		if (ticket.ExitedAt is not null)
		{
			return (null, (ExitOutcome.AlreadyExited, AlreadyExitedMessage));
		}

		// The current version (newest) and, if different, the version of the entry
		var currentVersionId = await _dbContext.TariffVersions
			.Where(version => version.BranchId == branchId)
			.OrderByDescending(version => version.CreatedAt)
			.Select(version => (Guid?)version.Id)
			.FirstOrDefaultAsync();

		var versionIds = new[] { ticket.TariffVersionId, currentVersionId }
			.Where(id => id is not null)
			.Select(id => id!.Value)
			.Distinct()
			.ToList();

		var versions = await _dbContext.TariffVersions
			.AsNoTracking()
			.Include(version => version.Rates)
			.Include(version => version.Packages)
			.Where(version => versionIds.Contains(version.Id))
			.ToListAsync();

		// Entry version first: it is the default choice when the prices changed
		versions = versions
			.OrderByDescending(version => version.Id == ticket.TariffVersionId)
			.ToList();

		var holidays = await LoadHolidaysAsync();

		var entryPackage = versions
			.SelectMany(version => version.Packages)
			.FirstOrDefault(package => package.Id == ticket.OvernightPackageId);

		var context = new ExitContext
		{
			TicketId = ticket.Id,
			SpotId = ticket.ParkingSpotId,
			SpotNumber = ticket.Spot.Number,
			SpotSize = ticket.Spot.Size,
			LicensePlate = ticket.LicensePlate,
			PlateKind = ticket.PlateKind,
			EnteredAt = ticket.EnteredAt,
			EntryVersionId = ticket.TariffVersionId,
			CurrentVersionId = currentVersionId,
			EntryPackage = entryPackage,
			Versions = versions,
			Holidays = holidays
		};

		return (context, null);
	}

	/// <summary>The company's holiday calendar: its marked dates and the Sunday setting.</summary>
	private async Task<HolidayCalendar> LoadHolidaysAsync()
	{
		var companyId = _tenantProvider.CurrentCompanyId
			?? throw new InvalidOperationException("No tenant context for an exit.");

		// Companies have no tenant filter (they ARE the tenants), so filter by id explicitly
		var sundaysAreHolidays = await _dbContext.Companies
			.Where(company => company.Id == companyId)
			.Select(company => company.SundaysAreHolidays)
			.FirstAsync();

		var dates = await _dbContext.Holidays
			.Select(holiday => holiday.Date)
			.ToListAsync();

		return new HolidayCalendar(dates.ToHashSet(), sundaysAreHolidays);
	}

	#endregion

	#region Charging

	/// <summary>
	/// Every allowed way to charge the ticket at a moment: per version (entry first), and for an
	/// exceeded package both overstay rules when they give different amounts.
	/// </summary>
	private List<ExitChargeOptionDto> BuildOptions(ExitContext context, DateTimeOffset exitAt)
	{
		var options = new List<ExitChargeOptionDto>();

		foreach (var version in context.Versions)
		{
			var withPriceList = Charge(context, version, PackageOverstay.PriceList, exitAt);

			// Null: this version no longer offers the ticket's package, so it cannot be chosen
			if (withPriceList is null)
			{
				continue;
			}

			var renewed = context.EntryPackage is null
				? null
				: Charge(context, version, PackageOverstay.RenewPackage, exitAt);

			// One option when the overstay rule makes no difference (no package, or no overstay yet)
			var overstayMatters = renewed is not null && renewed != withPriceList;

			options.Add(Option(context, version, overstayMatters ? PackageOverstay.PriceList : null, withPriceList.Value));

			if (overstayMatters)
			{
				options.Add(Option(context, version, PackageOverstay.RenewPackage, renewed!.Value));
			}
		}

		return options;
	}

	/// <summary>
	/// The amount for one version and overstay rule, or null when the ticket has a package that
	/// this version does not offer. A package is found again in a newer version by name and size.
	/// </summary>
	/// <exception cref="InvalidOperationException">The version has no prices for the spot size.</exception>
	private decimal? Charge(ExitContext context, TariffVersion version, PackageOverstay overstay, DateTimeOffset exitAt)
	{
		var tariff = TariffSnapshotFactory.From(version);

		if (context.EntryPackage is null)
		{
			return _calculator.CalculatePriceList(tariff, context.SpotSize, context.Holidays, context.EnteredAt, exitAt);
		}

		var package = version.Packages.FirstOrDefault(item =>
			item.Id == context.EntryPackage.Id
			|| (item.Name == context.EntryPackage.Name && item.Size == context.EntryPackage.Size));

		if (package is null)
		{
			return null;
		}

		return _calculator.CalculatePackage(
			tariff,
			context.SpotSize,
			context.Holidays,
			new PackageSnapshot(package.Nights, package.Price),
			overstay,
			context.EnteredAt,
			exitAt);
	}

	/// <summary>Builds one charge option for the quote.</summary>
	private static ExitChargeOptionDto Option(
		ExitContext context, TariffVersion version, PackageOverstay? overstay, decimal amount) => new()
	{
		TariffVersionId = version.Id,
		VersionCreatedAt = version.CreatedAt,
		IsEntryVersion = version.Id == context.EntryVersionId,
		IsCurrentVersion = version.Id == context.CurrentVersionId,
		Overstay = overstay,
		Amount = amount,
		VatRate = version.VatRate
	};

	#endregion

	#region Helpers

	private const string AlreadyExitedMessage = "Το όχημα έχει ήδη βγει (ίσως από άλλο ταμείο).";

	private const string NoPriceListMessage =
		"Δεν υπάρχει τιμοκατάλογος για αυτό το υποκατάστημα. Ζητήστε από τον ιδιοκτήτη να τον ορίσει.";

	/// <summary>The message when the price list has no prices for the spot size.</summary>
	private static string MissingSizeMessage(SpotSize size) =>
		$"Ο τιμοκατάλογος δεν έχει τιμές για θέση «{SpotSizeInfo.NameOf(size)}». Ζητήστε από τον ιδιοκτήτη να τις ορίσει.";

	/// <summary>The caller's active branch; the ActiveBranch policy guarantees it exists.</summary>
	private Guid ActiveBranchId() =>
		_activeBranchProvider.CurrentBranchId
			?? throw new InvalidOperationException(
				"No active branch in the current token. Protect the endpoint with the ActiveBranch policy.");

	/// <summary>Everything about one active ticket that charging needs, loaded once.</summary>
	private sealed class ExitContext
	{
		public Guid TicketId { get; init; }
		public Guid SpotId { get; init; }
		public required string SpotNumber { get; init; }
		public SpotSize SpotSize { get; init; }
		public required string LicensePlate { get; init; }
		public PlateKind PlateKind { get; init; }
		public DateTimeOffset EnteredAt { get; init; }
		public Guid? EntryVersionId { get; init; }
		public Guid? CurrentVersionId { get; init; }

		/// <summary>The package chosen at entry (from the entry version), or null.</summary>
		public OvernightPackage? EntryPackage { get; init; }

		/// <summary>The versions the ticket may be charged with, entry version first.</summary>
		public required List<TariffVersion> Versions { get; init; }

		public required HolidayCalendar Holidays { get; init; }

		/// <summary>The ticket as the console sees it (still active).</summary>
		public TicketDto ToDto() => new()
		{
			Id = TicketId,
			SpotId = SpotId,
			SpotNumber = SpotNumber,
			LicensePlate = LicensePlate,
			PlateKind = PlateKind,
			EnteredAt = EnteredAt,
			OvernightPackageName = EntryPackage?.Name
		};
	}

	#endregion
}
