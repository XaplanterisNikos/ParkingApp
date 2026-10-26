using Microsoft.EntityFrameworkCore;
using ParkingApp.Api.Data;
using ParkingApp.Api.Data.Entities;
using ParkingApp.Api.MultiTenancy;
using ParkingApp.Shared.Pricing;
using ParkingApp.Shared.Spots;

namespace ParkingApp.Api.Services.Pricing;

/// <summary>
/// Default <see cref="ITariffService"/>. A saved version is never updated: every save adds
/// a new <see cref="TariffVersion"/> with its own rates and packages.
/// </summary>
public class TariffService : ITariffService
{
	// The only steps the owner can choose (each divides 24 hours exactly)
	private static readonly int[] AllowedSteps = { 15, 30, 60 };

	// Database access; tenant (company) filtering is automatic via global query filters
	private readonly ParkingDbContext _dbContext;

	// The company of the caller, needed to stamp the new rows
	private readonly ITenantProvider _tenantProvider;

	/// <summary>
	/// Creates the service.
	/// </summary>
	/// <param name="dbContext">The application's database context.</param>
	/// <param name="tenantProvider">Provides the caller's company from the token.</param>
	public TariffService(ParkingDbContext dbContext, ITenantProvider tenantProvider)
	{
		_dbContext = dbContext;
		_tenantProvider = tenantProvider;
	}

	/// <inheritdoc />
	public async Task<TariffDto?> GetCurrentAsync(Guid branchId)
	{
		// The tenant filter makes another company's branch look missing
		if (!await _dbContext.Branches.AnyAsync(branch => branch.Id == branchId))
		{
			return null;
		}

		// Current = newest version of the branch
		var version = await _dbContext.TariffVersions
			.AsNoTracking()
			.Include(item => item.Rates)
			.Include(item => item.Packages)
			.Where(item => item.BranchId == branchId)
			.OrderByDescending(item => item.CreatedAt)
			.FirstOrDefaultAsync();

		return version is null ? Defaults() : ToDto(version);
	}

	/// <inheritdoc />
	public async Task<TariffSaveResult> SaveAsync(Guid branchId, SaveTariffRequest request)
	{
		var companyId = _tenantProvider.CurrentCompanyId
			?? throw new InvalidOperationException("No tenant context for saving a price list.");

		if (!await _dbContext.Branches.AnyAsync(branch => branch.Id == branchId))
		{
			return new TariffSaveResult(TariffSaveOutcome.BranchNotFound, null, new List<string>());
		}

		// The sizes the branch really has: each needs regular prices, or exit could not charge
		var sizesInBranch = await _dbContext.ParkingSpots
			.Where(spot => _dbContext.Floors.Any(floor => floor.Id == spot.FloorId && floor.BranchId == branchId))
			.Select(spot => spot.Size)
			.Distinct()
			.ToListAsync();

		var errors = Validate(request, sizesInBranch);

		if (errors.Count > 0)
		{
			return new TariffSaveResult(TariffSaveOutcome.Invalid, null, errors);
		}

		// A new version with its own copies of rates and packages; EF sets the foreign keys
		var version = new TariffVersion
		{
			CompanyId = companyId,
			BranchId = branchId,
			CreatedAt = DateTimeOffset.UtcNow,
			VatRate = request.VatRate,
			StepMinutes = request.StepMinutes,
			GraceMinutes = request.GraceMinutes,
			NightStart = request.NightStart,
			NightEnd = request.NightEnd,
			OvernightCheckoutTime = request.OvernightCheckoutTime,
			Rates = request.Rates
				.Select(rate => new TariffRate
				{
					CompanyId = companyId,
					Size = rate.Size,
					DayType = rate.DayType,
					DayStepPrice = rate.DayStepPrice,
					NightStepPrice = rate.NightStepPrice,
					DailyCap = rate.DailyCap
				})
				.ToList(),
			Packages = request.Packages
				.Select(package => new OvernightPackage
				{
					CompanyId = companyId,
					Name = package.Name.Trim(),
					Size = package.Size,
					Nights = package.Nights,
					Price = package.Price
				})
				.ToList()
		};

		_dbContext.TariffVersions.Add(version);
		await _dbContext.SaveChangesAsync();   // one save = one transaction

		return new TariffSaveResult(TariffSaveOutcome.Saved, ToDto(version), new List<string>());
	}

	/// <summary>Checks every rule of a price list and returns the broken ones, in words for the owner.</summary>
	/// <param name="request">The price list to check.</param>
	/// <param name="sizesInBranch">The spot sizes that exist in the branch.</param>
	private static List<string> Validate(SaveTariffRequest request, List<SpotSize> sizesInBranch)
	{
		var errors = new List<string>();

		if (request.VatRate < 0 || request.VatRate > 100)
			errors.Add("Ο ΦΠΑ πρέπει να είναι από 0 έως 100%.");

		if (!AllowedSteps.Contains(request.StepMinutes))
			errors.Add("Το βήμα χρέωσης πρέπει να είναι 15, 30 ή 60 λεπτά.");

		if (request.GraceMinutes < 0 || request.GraceMinutes > 120)
			errors.Add("Τα λεπτά χάριτος πρέπει να είναι από 0 έως 120.");

		// --- Rates ---
		if (request.Rates.Any(rate => !Enum.IsDefined(rate.Size) || !Enum.IsDefined(rate.DayType)))
			errors.Add("Υπάρχει τιμή με άγνωστο μέγεθος ή είδος ημέρας.");

		if (request.Rates.GroupBy(rate => (rate.Size, rate.DayType)).Any(group => group.Count() > 1))
			errors.Add("Κάθε μέγεθος έχει μία μόνο σειρά τιμών ανά είδος ημέρας.");

		if (request.Rates.Any(rate => rate.DayStepPrice < 0 || rate.NightStepPrice < 0))
			errors.Add("Οι τιμές δεν μπορεί να είναι αρνητικές.");

		if (request.Rates.Any(rate => rate.DailyCap is <= 0))
			errors.Add("Το ανώτατο 24ώρου πρέπει να είναι θετικό ή κενό (χωρίς όριο).");

		foreach (var size in sizesInBranch)
		{
			if (!request.Rates.Any(rate => rate.Size == size && rate.DayType == DayType.Regular))
				errors.Add($"Λείπουν οι τιμές για «{SpotSizeInfo.NameOf(size)}», που υπάρχει σε θέσεις του υποκαταστήματος.");
		}

		// --- Packages ---
		foreach (var package in request.Packages)
		{
			var name = string.IsNullOrWhiteSpace(package.Name) ? "(χωρίς όνομα)" : package.Name.Trim();

			if (string.IsNullOrWhiteSpace(package.Name) || package.Name.Trim().Length > 100)
				errors.Add($"Πακέτο {name}: το όνομα είναι υποχρεωτικό, έως 100 χαρακτήρες.");

			if (!Enum.IsDefined(package.Size))
				errors.Add($"Πακέτο {name}: άγνωστο μέγεθος θέσης.");

			if (package.Nights < 1 || package.Nights > 30)
				errors.Add($"Πακέτο {name}: οι νύχτες πρέπει να είναι από 1 έως 30.");

			if (package.Price <= 0)
				errors.Add($"Πακέτο {name}: η τιμή πρέπει να είναι θετική.");
		}

		return errors;
	}

	/// <summary>Suggested values for a branch that has no price list yet (not saved).</summary>
	private static TariffDto Defaults() => new()
	{
		VatRate = 24m,
		StepMinutes = 60,
		GraceMinutes = 0,
		NightStart = new TimeOnly(0, 0),
		NightEnd = new TimeOnly(7, 0),
		OvernightCheckoutTime = new TimeOnly(12, 0),
		// One empty row of regular prices per size, ready to be filled in
		Rates = Enum.GetValues<SpotSize>()
			.Select(size => new TariffRateDto { Size = size, DayType = DayType.Regular })
			.ToList()
	};

	/// <summary>Maps a stored version to its DTO, rates and packages in a stable order.</summary>
	private static TariffDto ToDto(TariffVersion version) => new()
	{
		VersionId = version.Id,
		CreatedAt = version.CreatedAt,
		VatRate = version.VatRate,
		StepMinutes = version.StepMinutes,
		GraceMinutes = version.GraceMinutes,
		NightStart = version.NightStart,
		NightEnd = version.NightEnd,
		OvernightCheckoutTime = version.OvernightCheckoutTime,
		Rates = version.Rates
			.OrderBy(rate => rate.DayType)
			.ThenBy(rate => rate.Size)
			.Select(rate => new TariffRateDto
			{
				Size = rate.Size,
				DayType = rate.DayType,
				DayStepPrice = rate.DayStepPrice,
				NightStepPrice = rate.NightStepPrice,
				DailyCap = rate.DailyCap
			})
			.ToList(),
		Packages = version.Packages
			.OrderBy(package => package.Size)
			.ThenBy(package => package.Nights)
			.ThenBy(package => package.Name, StringComparer.CurrentCulture)
			.Select(package => new OvernightPackageDto
			{
				Id = package.Id,
				Name = package.Name,
				Size = package.Size,
				Nights = package.Nights,
				Price = package.Price
			})
			.ToList()
	};
}
