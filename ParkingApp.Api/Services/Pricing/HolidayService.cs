using Microsoft.EntityFrameworkCore;
using ParkingApp.Api.Data;
using ParkingApp.Api.Data.Entities;
using ParkingApp.Api.MultiTenancy;
using ParkingApp.Shared.Pricing;

namespace ParkingApp.Api.Services.Pricing;

/// <summary>
/// Default <see cref="IHolidayService"/>. Holidays are tenant entities (filtered automatically);
/// the Sunday setting lives on the company itself.
/// </summary>
public class HolidayService : IHolidayService
{
	// Database access; tenant (company) filtering is automatic via global query filters
	private readonly ParkingDbContext _dbContext;

	// The company of the caller
	private readonly ITenantProvider _tenantProvider;

	/// <summary>
	/// Creates the service.
	/// </summary>
	/// <param name="dbContext">The application's database context.</param>
	/// <param name="tenantProvider">Provides the caller's company from the token.</param>
	public HolidayService(ParkingDbContext dbContext, ITenantProvider tenantProvider)
	{
		_dbContext = dbContext;
		_tenantProvider = tenantProvider;
	}

	/// <inheritdoc />
	public async Task<HolidayCalendarDto> GetCalendarAsync()
	{
		var companyId = CurrentCompanyId();

		// Companies have no tenant filter (they ARE the tenants), so filter by id explicitly
		var sundaysAreHolidays = await _dbContext.Companies
			.Where(company => company.Id == companyId)
			.Select(company => company.SundaysAreHolidays)
			.FirstAsync();

		var holidays = await _dbContext.Holidays
			.AsNoTracking()
			.OrderBy(holiday => holiday.Date)
			.Select(holiday => new HolidayDto { Id = holiday.Id, Date = holiday.Date, Name = holiday.Name })
			.ToListAsync();

		return new HolidayCalendarDto { SundaysAreHolidays = sundaysAreHolidays, Holidays = holidays };
	}

	/// <inheritdoc />
	public async Task<HolidayDto?> AddAsync(CreateHolidayRequest request)
	{
		var companyId = CurrentCompanyId();

		// Friendly check first; the unique index (CompanyId, Date) is the real guard
		if (await _dbContext.Holidays.AnyAsync(holiday => holiday.Date == request.Date))
		{
			return null;
		}

		var holiday = new Holiday { CompanyId = companyId, Date = request.Date, Name = request.Name.Trim() };

		_dbContext.Holidays.Add(holiday);
		await _dbContext.SaveChangesAsync();

		return new HolidayDto { Id = holiday.Id, Date = holiday.Date, Name = holiday.Name };
	}

	/// <inheritdoc />
	public async Task<bool> DeleteAsync(Guid holidayId)
	{
		// One statement; the tenant filter keeps another company's holiday out of reach.
		// Deleting is safe: charged tickets keep their amount, they do not point to holidays.
		var deleted = await _dbContext.Holidays
			.Where(holiday => holiday.Id == holidayId)
			.ExecuteDeleteAsync();

		return deleted > 0;
	}

	/// <inheritdoc />
	public async Task SetSundaysAreHolidaysAsync(bool sundaysAreHolidays)
	{
		var companyId = CurrentCompanyId();

		await _dbContext.Companies
			.Where(company => company.Id == companyId)
			.ExecuteUpdateAsync(setters => setters.SetProperty(company => company.SundaysAreHolidays, sundaysAreHolidays));
	}

	/// <summary>The caller's company; every endpoint here requires one.</summary>
	private Guid CurrentCompanyId() =>
		_tenantProvider.CurrentCompanyId
			?? throw new InvalidOperationException("No tenant context for the holiday calendar.");
}
